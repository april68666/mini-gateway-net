namespace mini_gateway.config;

public struct Http
{
    public int Port { get; set; }
    public Middleware[] Middlewares { get; set; }
    public Endpoint[] Endpoints { get; set; }
}