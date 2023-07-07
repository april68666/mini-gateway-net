namespace mini_gateway.discovery;

public delegate void CallBack(Result result);

public interface IResolver
{
    public Result Resolve(string desc,CancellationToken cancellationToken);
    public void Watch(string desc,CallBack callBack,CancellationToken cancellationToken);
}