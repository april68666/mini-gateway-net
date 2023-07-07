using mini_gateway.config;
using mini_gateway.http;

namespace mini_gateway.router;

public class Route
{
    public IHandler Handler { get; }
    private readonly Predicates _predicates;

    public Route(Predicates predicates, IHandler handler)
    {
        _predicates = predicates;
        Handler = handler;
    }

    public string[] Path()
    {
        return _predicates.Path.Split(',');
    }

    public bool Match(HttpRequest req)
    {
        return MatchMethod(req.Method) && MatchHeader(req.Headers);
    }

    private bool MatchMethod(string method)
    {
        if (method.Equals("OPTIONS"))
        {
            return true;
        }

        var ms = _predicates.Method.Trim().Split(',');
        return ms.Length > 0 && ms.Any(s => s.ToUpper().Equals(method.ToUpper()));
    }

    private bool MatchHeader(IHeaderDictionary headers)
    {
        return _predicates.Headers == null ||
               _predicates.Headers.All(keyValuePair => headers[keyValuePair.Key] == keyValuePair.Value);
    }
}