using Microsoft.Extensions.Logging;

namespace Frontend.Diagnostics;

/// <summary>
/// Logger para inicialização e código estático do host (antes ou fora do DI do <see cref="Microsoft.Maui.Hosting.MauiApp"/>).
/// </summary>
internal static class AppHostLogging
{
    private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder =>
    {
#if DEBUG
        builder.AddDebug();
#endif
        builder.SetMinimumLevel(LogLevel.Information);
    });

    public static ILogger Create(string categoryName) => Factory.CreateLogger(categoryName);
}
