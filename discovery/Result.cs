namespace mini_gateway.discovery;

public class Result
{
    public Result(INode[] nodes)
    {
        Nodes = nodes;
    }

    public INode[] Nodes { get; }
}