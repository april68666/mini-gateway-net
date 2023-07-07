namespace mini_gateway.http;

public static class HttpContextExtensions
{
    public static HttpRequestMessage HttpRequestMessage(this HttpRequest request)
    {
        var httpRequestMessage = new HttpRequestMessage();

        httpRequestMessage.RequestUri =
            new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");

        httpRequestMessage.Method = new HttpMethod(request.Method);

        httpRequestMessage.Content = new StreamContent(request.Body);

        foreach (var header in request.Headers)
        {
            if (!httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                httpRequestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        return httpRequestMessage;
    }

    public static void WithEndpoint(this HttpContext context, config.Endpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        context.Items.TryAdd("x-endpoint", endpoint);
    }

    public static config.Endpoint? Endpoint(this HttpContext context)
    {
        if (context.Items.TryGetValue("x-endpoint", out var obj))
        {
            return (config.Endpoint)obj!;
        }

        return null;
    }

    public static void WithParams(this HttpContext context, IDictionary<string, string> paramsMap)
    {
        context.Items.TryAdd("x-params", paramsMap);
    }
    
    public static Dictionary<string,string>? Params(this HttpContext context)
    {
        if (context.Items.TryGetValue("x-params", out var obj))
        {
            return (Dictionary<string,string>)obj!;
        }

        return null;
    }
}