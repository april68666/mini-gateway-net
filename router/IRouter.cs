using mini_gateway.http;

namespace mini_gateway.router;

public interface IRouter:IHandler
{
    void RegisterOrUpdateRoutes(IEnumerable<Route> rs);
}