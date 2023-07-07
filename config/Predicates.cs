namespace mini_gateway.config;

public struct Predicates
{
    public Predicates()
    {
    }

    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public IDictionary<string, object>? Headers { get; set; } = null;
}