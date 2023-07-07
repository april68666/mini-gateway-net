using static System.Int32;

namespace mini_gateway.middleware;

public class StripPrefix : http.IRoundTripper
{
    private readonly http.IRoundTripper _roundTripper;
    private readonly int _call;

    public StripPrefix(config.Middleware conf, http.IRoundTripper roundTripper)
    {
        _roundTripper = roundTripper;
        var call = 0;
        if (conf.Args != null && conf.Args.TryGetValue("call", out var obj))
        {
            if (!TryParse(obj.ToString(), out call))
            {
                call = 0;
            }
        }

        _call = call;
    }

    public async Task<HttpResponseMessage> RoundTripperAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var ps = request.Path.ToString().TrimStart('/').Split('/');
        if (ps.Length >= _call)
        {
            request.Path = new PathString($"/{string.Join(',', ps.Skip(_call))}");
        }

        return await _roundTripper.RoundTripperAsync(request, cancellationToken);
    }
}