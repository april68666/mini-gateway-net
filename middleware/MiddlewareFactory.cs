using System.Collections.Concurrent;

namespace mini_gateway.middleware;

public delegate Middleware Factory(config.Middleware conf);

public delegate http.IRoundTripper Middleware(http.IRoundTripper router);

public static class MiddlewareFactory
{
    static MiddlewareFactory()
    {
        Register("stripPrefix", (conf => (router => new StripPrefix(conf, router))));
        Register("logging", (conf => (router => new Logging(conf, router))));
        Register("cors", (conf => (router => new Cors(conf, router))));
    }

    private static readonly ConcurrentDictionary<string, Factory> Map = new();

    public static void Register(string name, Factory factory)
    {
        Map.TryAdd(name, factory);
    }

    public static Factory? GetMiddleware(string name)
    {
        return Map.TryGetValue(name, out var factory) ? factory : null;
    }

    public static http.IRoundTripper BuildMiddleware(IEnumerable<config.Middleware> ms, http.IRoundTripper next)
    {
        var msList = ms.ToList();
        msList.Sort(((x, y) => x.Order.CompareTo(y.Order)));
        foreach (var msConf in msList)
        {
            var factory = GetMiddleware(msConf.Name);
            if (factory == null)
            {
                continue;
            }

            var middleware = factory(msConf);
            next = middleware(next);
        }

        return next;
    }
}