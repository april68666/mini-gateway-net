namespace mini_gateway.proxy;

public struct RouteInfo
{
    public CancellationTokenSource CancellationToken { get; }
    public router.Route Route { get; }
    public config.Endpoint Endpoint { get; }

    public RouteInfo(CancellationTokenSource cancellationToken,router.Route route,config.Endpoint endpoint)
    {
        CancellationToken = cancellationToken;
        Route = route;
        Endpoint = endpoint;
    }
}