using Serilog;
using Serilog.Events;
using System.Text;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Serilog 扩展
/// </summary>
public static class SerilogHostExtensions
{
    public static IHostBuilder UseSerilogEx(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseSerilog((context, configuration) =>
        {
            var config = configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();

            var hasWriteTo = context.Configuration["Serilog:WriteTo:0:Name"];

            if (hasWriteTo != null) return;

            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message:lj}{NewLine}{Exception}";
            config.WriteTo.Console(outputTemplate: outputTemplate).WriteTo.Logger(fileLogger =>
            {
                fileLogger.Filter.ByIncludingOnly(p => p.Level.Equals(LogEventLevel.Debug)).WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs/debug", "debug.log"),
                    LogEventLevel.Debug,
                    outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    encoding: Encoding.UTF8);
            }).WriteTo.Logger(fileLogger =>
            {
                fileLogger.Filter.ByIncludingOnly(p => p.Level.Equals(LogEventLevel.Information)).WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs/info", "info.log"),
                    LogEventLevel.Information,
                    outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    encoding: Encoding.UTF8);
            }).WriteTo.Logger(fileLogger =>
            {
                fileLogger.Filter.ByIncludingOnly(p => p.Level.Equals(LogEventLevel.Warning)).WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs/warn", "warn.log"),
                    LogEventLevel.Warning,
                    outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    encoding: Encoding.UTF8);
            }).WriteTo.Logger(fileLogger =>
            {
                fileLogger.Filter.ByIncludingOnly(p => p.Level.Equals(LogEventLevel.Error)).WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs/error", "error.log"),
                    LogEventLevel.Error,
                    outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    encoding: Encoding.UTF8);
            }).WriteTo.Logger(fileLogger =>
            {
                fileLogger.Filter.ByIncludingOnly(p => p.Level.Equals(LogEventLevel.Fatal)).WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs/fatal", "fatal.log"),
                    LogEventLevel.Fatal,
                    outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null,
                    encoding: Encoding.UTF8);
            });
        });
        return builder;
    }
}