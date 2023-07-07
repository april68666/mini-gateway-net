using mini_gateway.discovery;

namespace mini_gateway.loadBalance;

public class RotationPicker : IPicker
{
    private volatile int _index = -1;
    private volatile INode[]? _nodes;

    public INode? Next()
    {
        var nodes = _nodes;
        if (nodes == null || nodes.Length == 0)
        {
            return null;
        }

        int index;
        do
        {
            index = Interlocked.Increment(ref _index);
            if (index < nodes.Length)
            {
                return nodes[index];
            }
        } while (Interlocked.CompareExchange(ref _index, -1, index) != _index);

        return nodes[0];
    }

    public void Apply(INode[] nodes)
    {
        Interlocked.Exchange(ref _nodes, nodes);
    }
}