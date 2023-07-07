using mini_gateway.http;
using mini_gateway.loadBalance;

namespace mini_gateway.client;

public class Client : IRoundTripper
{
    private readonly IPicker _picker;
    private readonly HttpClient _httpClient;

    public Client(IPicker picker, HttpClient httpClient)
    {
        _picker = picker;
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> RoundTripperAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var node = _picker.Next();
        if (node == null)
        {
            throw new Exception("no target node");
        }

        if (request.HttpContext.WebSockets.IsWebSocketRequest)
        {
            return await new WebsocketWarp(node, request).AcceptWebSocketAsync(cancellationToken);
        }

        var httpRequestMessage = request.HttpRequestMessage();
        httpRequestMessage.RequestUri = new Uri($"{node.Uri.OriginalString}{request.Path}{request.QueryString}");

        return await _httpClient.SendAsync(httpRequestMessage, cancellationToken);
    }
}