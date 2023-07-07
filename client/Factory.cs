using mini_gateway.discovery;
using mini_gateway.loadBalance;

namespace mini_gateway.client;

public delegate http.IRoundTripper Factory(config.Endpoint endpoint, CancellationToken cancellationToken);

public static class ClientFactory
{
    private static HttpClient HttpClient { get; } = new();

    public static Factory CreateFactory(IResolver? resolver)
    {
        return (endpoint, context) =>
        {
            var client = HttpClient;
            if (endpoint.Protocol.ToLower().Equals("grpc"))
            {
                // TODO 替换成 http2 client
            }

            var factory = PickerFactory.GetPicker(endpoint.LoadBalance);
            var picker = factory != null ? factory() : new RotationPicker();

            var discovery = endpoint.Discovery.Trim();
            if (resolver != null && discovery.Length > 0)
            {
                var result = resolver.Resolve(discovery, context);
                picker.Apply(result.Nodes);

                resolver.Watch(discovery, (res => { picker.Apply(res.Nodes); }), context);
            }
            else
            {
                var ns = endpoint.Targets.Select(target => new Node(target.Uri, target.Weight, target.Tags))
                    .Cast<INode>().ToList();
                picker.Apply(ns.ToArray());
            }

            return new Client(picker, client);
        };
    }
}