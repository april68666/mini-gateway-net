namespace mini_gateway.router;

public class RouteTrie<T>
{
    private readonly Node<T> _node;

    public RouteTrie()
    {
        _node = new Node<T>(default, string.Empty, false, new Dictionary<string, Node<T>>());
    }

    public void Insert(string path, T? value)
    {
        var n = _node;
        path = path.Trim('/');
        foreach (var s in path.Split('/'))
        {
            if (!n.Children.ContainsKey(s))
            {
                n.Children[s] = new Node<T>(default, s, false, new Dictionary<string, Node<T>>());
            }

            if (s.Equals("*") || s.StartsWith("{") && s.EndsWith("}"))
            {
                n.WildCard = true;
            }

            n = n.Children[s];
        }

        n.Value = value;
    }

    public (IDictionary<string, string>, T?, bool) Search(string path)
    {
        var n = _node;
        Dictionary<string, string> paramsMap = new();
        path = path.Trim('/');
        foreach (var s in path.Split('/'))
        {
            var str = s;
            if (n.WildCard)
            {
                foreach (var node in n.Children)
                {
                    if (node.Key.StartsWith("{") && node.Key.EndsWith("}"))
                    {
                        var key = node.Key.TrimStart('{').TrimEnd('}');
                        paramsMap[key] = s;
                    }

                    str = node.Key;
                }
            }

            if (!n.Children.ContainsKey(str))
            {
                return (paramsMap, default, false);
            }

            n = n.Children[str];
        }

        if (n.Children.Count == 0)
        {
            return (paramsMap, n.Value, true);
        }

        return (paramsMap,default,false);
    }
}

public class Node<T>
{
    public T? Value;
    public readonly string Path;
    public bool WildCard;
    public readonly Dictionary<string, Node<T>> Children;

    public Node(T? value, string path, bool wildCard, Dictionary<string, Node<T>> children)
    {
        Value = value;
        Path = path;
        WildCard = wildCard;
        Children = children;
    }
}