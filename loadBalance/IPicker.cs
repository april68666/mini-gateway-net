namespace mini_gateway.loadBalance;

public interface IPicker
{
    public discovery.INode? Next();
    public void Apply(discovery.INode[] nodes);
}