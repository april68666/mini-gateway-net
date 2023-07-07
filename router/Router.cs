using System.Net;
using mini_gateway.http;

namespace mini_gateway.router;

public class Router : IRouter
{
    private volatile RouteTrie<Route> _trie = new();

    public async Task ServeHttpAsync(HttpContext httpContext)
    {
        var (p, re, exits) = _trie.Search(httpContext.Request.Path.ToString());
        if (exits && re != null)
        {
            if (re.Match(httpContext.Request))
            {
                if (p.Count > 0)
                {
                    httpContext.WithParams(p);
                }

                await re.Handler.ServeHttpAsync(httpContext);
                return;
            }
        }
        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        await httpContext.Response.WriteAsync("Not Found");
    }

    public void RegisterOrUpdateRoutes(IEnumerable<Route> rs)
    {
        var trie = new RouteTrie<Route>();
        foreach (var route in rs)
        {
            foreach (var path in route.Path())
            {
                trie.Insert(path, route);
            }
        }

        Interlocked.Exchange(ref _trie, trie);
    }
}