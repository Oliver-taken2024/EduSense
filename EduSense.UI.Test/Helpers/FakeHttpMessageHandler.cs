using System.Net;
using System.Net.Http.Json;

namespace EduSense.UI.Test.Helpers;

// Enkel handler som returnerar ett förkonfigurerat svar, utan att göra riktiga HTTP-anrop.
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly object? _content;

    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeHttpMessageHandler(HttpStatusCode statusCode, object? content = null)
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        var response = new HttpResponseMessage(_statusCode);
        if (_content is not null)
        {
            response.Content = JsonContent.Create(_content);
        }

        return Task.FromResult(response);
    }
}