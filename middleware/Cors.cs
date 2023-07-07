using System.Net;

namespace mini_gateway.middleware;

public class Cors : http.IRoundTripper
{
    private readonly http.IRoundTripper _roundTripper;
    private readonly string _allowOrigin = "*";
    private readonly string _allowHeaders = "*";
    private readonly string _allowMethod = "*";
    private readonly string _exposeHeaders = "*";
    private readonly string _credentials = "true";

    public Cors(config.Middleware conf, http.IRoundTripper roundTripper)
    {
        if (conf.Args != null)
        {
            if (conf.Args.TryGetValue("allowOrigin", out var allowOrigin))
            {
                _allowOrigin = allowOrigin.ToString()!;
            }

            if (conf.Args.TryGetValue("allowHeaders", out var allowHeaders))
            {
                _allowHeaders = allowHeaders.ToString()!;
            }

            if (conf.Args.TryGetValue("allowMethod", out var allowMethod))
            {
                _allowMethod = allowMethod.ToString()!;
            }

            if (conf.Args.TryGetValue("exposeHeaders", out var exposeHeaders))
            {
                _exposeHeaders = exposeHeaders.ToString()!;
            }

            if (conf.Args.TryGetValue("credentials", out var credentials))
            {
                _credentials = credentials.ToString()!;
            }
        }

        _roundTripper = roundTripper;
    }

    public async Task<HttpResponseMessage> RoundTripperAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!request.Method.Equals("OPTIONS"))
        {
            var httpResponseMessage = await _roundTripper.RoundTripperAsync(request, cancellationToken);
            if (request.Headers.TryGetValue("Sec-Fetch-Mode", out var header))
            {
                if (header == "cors")
                {
                    httpResponseMessage.Headers.TryAddWithoutValidation("Access-Control-Allow-Origin", "*");
                }
            }

            return httpResponseMessage;
        }

        var res = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NoContent,
        };
        res.Headers.TryAddWithoutValidation("Access-Control-Allow-Origin", _allowOrigin);
        res.Headers.TryAddWithoutValidation("Access-Control-Allow-Headers", _allowHeaders);
        res.Headers.TryAddWithoutValidation("Access-Control-Allow-Methods", _allowMethod);
        res.Headers.TryAddWithoutValidation("Access-Control-Expose-Headers", _exposeHeaders);
        res.Headers.TryAddWithoutValidation("Access-Control-Allow-Credentials", _credentials);
        return res;
    }
}