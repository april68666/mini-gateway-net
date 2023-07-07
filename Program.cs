using mini_gateway.client;
using mini_gateway.config;
using mini_gateway.proxy;
using mini_gateway.router;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilogEx();

var app = builder.Build();

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(UnderscoredNamingConvention.Instance)
    .Build();

var yaml = File.ReadAllText(app.Configuration["GatewayConfig"]!);

var conf = deserializer.Deserialize<GatewayConfig>(yaml);

var proxy = new Proxy(ClientFactory.CreateFactory(null), new Router());
proxy.UpdateEndpoints(conf.Http.Middlewares, conf.Http.Endpoints);
app.UseWebSockets();

app.Use(new Func<HttpContext, RequestDelegate, Task>(async (context, _) => { await proxy.ServeHttpAsync(context); }));
app.Run($"http://127.0.0.1:{conf.Http.Port}");