namespace EduSense.UI.Test.Helpers;


// Fejkad HttpMessageHandler som kan ge olika svar beroende på request,
// via en uppslagsfunktion. 

public class RoutingFakeHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];


    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(responder(request));
    }
}