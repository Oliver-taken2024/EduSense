using System.Net;
using Microsoft.AspNetCore.Components.WebAssembly.Http; // extension för SetBrowserRequestCredentials

namespace EduSense.UI.Http
{
    // Fångar 401 orsakat av att den kortlivade access-token-cookien (15 min, se AuthController)
    // gått ut, försöker förnya den via /api/auth/refresh (refresh-token-cookien lever 7 dagar),
    // och gör om originalanropet en gång - istället för att tvinga omlogin var 15:e minut.
    public class CookieHandler : DelegatingHandler
    {
        private readonly object _refreshLock = new();
        private Task<bool>? _refreshTask;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            // Buffra body innan första sändning - request.Content kan annars inte läsas
            // igen om vi behöver skicka om samma anrop efter en lyckad refresh.
            byte[]? bodyForRetry = request.Content is null
                ? null
                : await request.Content.ReadAsByteArrayAsync(cancellationToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized || IsAuthEndpoint(request.RequestUri))
            {
                return response;
            }

            if (!await RefreshAccessTokenAsync(request.RequestUri!, cancellationToken))
            {
                return response; // refresh-token också utgången/ogiltig - låt 401:an gå igenom, UI loggar ut
            }

            response.Dispose();

            var retryRequest = CloneRequest(request, bodyForRetry);
            retryRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private static bool IsAuthEndpoint(Uri? uri) =>
            uri is not null && (uri.AbsolutePath.EndsWith("/api/auth/refresh", StringComparison.OrdinalIgnoreCase)
                              || uri.AbsolutePath.EndsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase));

        // Flera samtidiga anrop kan få 401 innan token hunnit förnyas - låt dem alla vänta
        // på SAMMA refresh-anrop istället för att trigga flera parallella (refresh-token
        // roteras och blir engångsbruk server-side, se AuthController.Refresh).
        private Task<bool> RefreshAccessTokenAsync(Uri originalUri, CancellationToken cancellationToken)
        {
            lock (_refreshLock)
            {
                return _refreshTask ??= DoRefreshAsync(originalUri, cancellationToken);
            }
        }

        private async Task<bool> DoRefreshAsync(Uri originalUri, CancellationToken cancellationToken)
        {
            try
            {
                var refreshRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(originalUri, "/api/auth/refresh"));
                refreshRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                using var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);
                return refreshResponse.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
            finally
            {
                lock (_refreshLock)
                {
                    _refreshTask = null;
                }
            }
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? body)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);

            if (body is not null)
            {
                clone.Content = new ByteArrayContent(body);
                foreach (var header in original.Content!.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var header in original.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }
}
