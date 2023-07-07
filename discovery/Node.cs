namespace mini_gateway.discovery;

public class Node : INode
{
    public Node(string uri, int weight, IDictionary<string, string>? tags)
    {
        Uri = new Uri(uri);
        Weight = weight;
        Tags = tags;
    }

    public Uri Uri { get; }
    public int Weight { get; }
    private IDictionary<string, string>? Tags { get; }

    public string? Tag(string key)
    {
        if (Tags == null)
        {
            return null;
        }

        return Tags.TryGetValue(key, out var res) ? res : null;
    }
}