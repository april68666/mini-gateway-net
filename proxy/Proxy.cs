using System.Net;
using Microsoft.Net.Http.Headers;
using mini_gateway.client;
using mini_gateway.config;
using mini_gateway.http;

namespace mini_gateway.proxy;

public class Proxy : IHandler
{
    private static readonly object Lock = new();
    private Dictionary<string, RouteInfo> _routeInfo = new();
    private readonly Factory _factory;
    private readonly router.IRouter _router;

    public Proxy(Factory factory, router.IRouter router)
    {
        _factory = factory;
        _router = router;
    }

    public async Task ServeHttpAsync(HttpContext httpContext)
    {
        await _router.ServeHttpAsync(httpContext);
    }

    public void UpdateEndpoints(Middleware[] globalMs, IEnumerable<config.Endpoint> es)
    {
        lock (Lock) // TODO ??
        {
            var rs = new List<router.Route>();
            var ris = new Dictionary<string, RouteInfo>();
            foreach (var e in es)
            {
                if (ris.ContainsKey(e.Id))
                {
                    throw new ArgumentException($"endpoint id cannot be the same,id:{e.Id}");
                }

                var ctx = new CancellationTokenSource();
                var handler = BuildEndpoints(globalMs, e, ctx.Token);
                var route = new router.Route(e.Predicates, handler);
                ris.TryAdd(e.Id, new RouteInfo(ctx, route, e));
                rs.Add(route);
            }

            _router.RegisterOrUpdateRoutes(rs.ToArray());
            foreach (var (_, value) in _routeInfo)
            {
                value.CancellationToken.Cancel();
            }

            _routeInfo = ris;
        }
    }

    private IHandler BuildEndpoints(IEnumerable<Middleware> ms, config.Endpoint endpoint,
        CancellationToken cancellationToken)
    {
        var factory = _factory(endpoint, cancellationToken);

        var tripper = middleware.MiddlewareFactory.BuildMiddleware(endpoint.Middlewares, factory);

        tripper = middleware.MiddlewareFactory.BuildMiddleware(ms, tripper);

        return new Endpoint(tripper, endpoint);
    }
}

public class Endpoint : IHandler
{
    private readonly IRoundTripper _tripper;
    private readonly config.Endpoint _endpoint;

    public Endpoint(IRoundTripper tripper, config.Endpoint endpoint)
    {
        _tripper = tripper;
        _endpoint = endpoint;
    }

    public async Task ServeHttpAsync(HttpContext httpContext)
    {
        httpContext.WithEndpoint(_endpoint);

        var ctx = new CancellationTokenSource();
        if (_endpoint.Timeout > 0)
        {
            ctx.CancelAfter(_endpoint.Timeout);
        }

        var reqUpType = UpgradeType(httpContext.Request.Headers);

        RemoveHopByHopHeaders(httpContext.Request.Headers);

        if (reqUpType != string.Empty)
        {
            httpContext.Request.Headers.TryAdd(HeaderNames.Connection, HeaderNames.Upgrade);
            httpContext.Request.Headers.TryAdd(HeaderNames.Upgrade, reqUpType);
        }

        SetXForwarded(httpContext.Request);

        var res = await _tripper.RoundTripperAsync(httpContext.Request, ctx.Token);

        if (res.StatusCode == HttpStatusCode.OK && res is WebsocketWarp message)
        {
            await message.Run(httpContext.RequestAborted);
            return;
        }

        CopyHeader(httpContext.Response, res);

        RemoveHopByHopHeaders(httpContext.Response.Headers);

        httpContext.Response.StatusCode = (int)res.StatusCode;

        await res.Content.CopyToAsync(httpContext.Response.Body, ctx.Token);
    }

    private static string UpgradeType(IHeaderDictionary headers)
    {
        return headers.Connection.Any(s => s != null && s.Equals(HeaderNames.Upgrade))
            ? headers.Upgrade.ToString()
            : string.Empty;
    }

    private static void RemoveHopByHopHeaders(IHeaderDictionary headers)
    {
        var hopHeaders = new[]
        {
            "Connection",
            "Proxy-Connection", // non-standard but still sent by libcurl and rejected by e.g. google
            "Keep-Alive",
            "Proxy-Authenticate",
            "Proxy-Authorization",
            "Te", // canonicalized version of "TE"
            "Trailer", // not Trailers per URL above; https://www.rfc-editor.org/errata_search.php?eid=4522
            "Transfer-Encoding",
            "Upgrade",
        };
        foreach (var str in headers.Connection)
        {
            headers.Remove(str!.Trim());
        }

        foreach (var str in hopHeaders)
        {
            headers.Remove(str.Trim());
        }
    }

    private static void SetXForwarded(HttpRequest request)
    {
        var clientIp = request.HttpContext.Connection.RemoteIpAddress?.ToString();
        var prior = request.Headers["X-Forwarded-For"].ToList();
        {
            clientIp = prior.Aggregate("", (current, s) => current + (s + ",")) + clientIp;
            request.Headers.TryAdd("X-Forwarded-For", clientIp);
        }

        request.Headers.TryAdd("X-Forwarded-Host", request.Host.Host);
    }

    private static void CopyHeader(HttpResponse dsc, HttpResponseMessage src)
    {
        foreach (var header in src.Headers)
        {
            dsc.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in src.Content.Headers)
        {
            dsc.Headers[header.Key] = header.Value.ToArray();
        }
    }
}