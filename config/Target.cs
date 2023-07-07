namespace mini_gateway.config;

public struct Target
{
    public Target()
    {
    }

    public string Uri { get; set; } = string.Empty;
    public int Weight { get; set; } = 0;
    public IDictionary<string, string>? Tags { get; set; } = null;
}