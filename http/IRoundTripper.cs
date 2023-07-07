namespace mini_gateway.http;

public interface IRoundTripper
{
    Task<HttpResponseMessage> RoundTripperAsync(HttpRequest request,CancellationToken cancellationToken);
}