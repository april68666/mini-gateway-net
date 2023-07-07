using System.Diagnostics;

namespace mini_gateway.middleware;

public class Logging : http.IRoundTripper
{
    private readonly http.IRoundTripper _roundTripper;

    public Logging(config.Middleware conf, http.IRoundTripper roundTripper)
    {
        _roundTripper = roundTripper;
    }

    public async Task<HttpResponseMessage> RoundTripperAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var logger = request.HttpContext.RequestServices.GetService<ILogger<Logging>>()!;
        var path = request.Path;
        var sw = new Stopwatch();
        sw.Restart();

        var res = await _roundTripper.RoundTripperAsync(request, cancellationToken);

        sw.Stop();
        logger.LogInformation("{Method} {Url} {ElapsedMilliseconds}ms", request.Method, path,
            sw.ElapsedMilliseconds);
        return res;
    }
}