using System.Net;
using System.Net.WebSockets;
using Microsoft.Net.Http.Headers;
using mini_gateway.discovery;
using mini_gateway.http;

namespace mini_gateway.client;

public class WebsocketWarp : HttpResponseMessage
{
    private readonly INode _node;
    private readonly HttpRequest _request;

    private WebSocket? _src;
    private WebSocket? _dst;

    private readonly ILogger<WebsocketWarp>? _logger;

    public WebsocketWarp(INode node, HttpRequest request)
    {
        _node = node;
        _request = request;
        _logger = request.HttpContext.RequestServices.GetService<ILogger<WebsocketWarp>>();
    }

    public async Task<HttpResponseMessage> AcceptWebSocketAsync(CancellationToken cancellationToken)
    {
        var client = new ClientWebSocket();

        foreach (var protocol in _request.HttpContext.WebSockets.WebSocketRequestedProtocols)
        {
            client.Options.AddSubProtocol(protocol);
        }

        var notForwardedHeaders = new HashSet<string>()
        {
            HeaderNames.Connection, HeaderNames.Host, HeaderNames.Upgrade,
            HeaderNames.SecWebSocketAccept, HeaderNames.SecWebSocketProtocol,
            HeaderNames.SecWebSocketVersion, HeaderNames.SecWebSocketExtensions,
            HeaderNames.SecWebSocketKey
        };
        foreach (var header in _request.Headers)
        {
            if (!notForwardedHeaders.Contains(header.Key))
            {
                client.Options.SetRequestHeader(header.Key, header.Value);
            }
        }

        var uri = new Uri($"ws://{_node.Uri.Host}:{_node.Uri.Port}{_request.Path}{_request.QueryString}");
        try
        {
            await client.ConnectAsync(uri, cancellationToken);
        }
        catch (WebSocketException ex)
        {
            var endpoint = _request.HttpContext.Endpoint();
            _logger?.LogError(ex, "connection server error, endpoint_id:{ID},uri:{Uri}", endpoint?.Id, uri);
            StatusCode = HttpStatusCode.BadGateway;
            return this;
        }

        _src = await _request.HttpContext.WebSockets.AcceptWebSocketAsync();
        _dst = client;
        return this;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(_src);
        ArgumentNullException.ThrowIfNull(_dst);
        try
        {
            await Task.WhenAll(Forward(_src, _dst, cancellationToken), Forward(_dst, _src, cancellationToken));
        }
        finally
        {
            _src?.Dispose();
            _dst?.Dispose();
        }
    }

    private static async Task Forward(WebSocket src, WebSocket dst, CancellationToken cancellationToken)
    {
        while (true)
        {
            var buffer = new byte[4096];
            WebSocketReceiveResult result;
            try
            {
                result = await src.ReceiveAsync(buffer, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await dst.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, "EndpointUnavailable",
                    new CancellationToken());
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await dst.CloseOutputAsync(result.CloseStatus!.Value, result.CloseStatusDescription,
                    new CancellationToken());
                return;
            }

            await dst.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType,
                result.EndOfMessage, cancellationToken);
        }
    }
}