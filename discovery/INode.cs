namespace mini_gateway.discovery;

public interface INode
{
    public Uri Uri { get; }
    public int Weight { get; }
    public string? Tag(string key);
}