namespace mini_gateway.http;

public interface IHandler
{
    Task ServeHttpAsync(HttpContext httpContext);
}