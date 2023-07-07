namespace mini_gateway.config;

public struct Middleware
{
    public string Name { get; set; }
    public int Order { get; set; }
    public IDictionary<string, object>? Args { get; set; }
}