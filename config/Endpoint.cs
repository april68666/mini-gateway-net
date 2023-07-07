namespace mini_gateway.config;

public struct Endpoint
{
    public Endpoint()
    {
        Targets = Array.Empty<Target>();
        Predicates = default;
        Middlewares = Array.Empty<Middleware>();
    }

    public string Id { get; set; } = string.Empty;
    public Target[] Targets { get; set; }
    public string Discovery { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public int Timeout { get; set; } = 0;
    public string LoadBalance { get; set; } = string.Empty;
    public Predicates Predicates { get; set; }
    public Middleware[] Middlewares { get; set; } 
}