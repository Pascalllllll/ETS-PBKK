using System.Diagnostics;
using Avalonia;
using Avalonia.Logging;

namespace ETC.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // ETC_LOG=1 menampilkan peringatan Avalonia di terminal.
        if (Environment.GetEnvironmentVariable("ETC_LOG") is not null)
            Trace.Listeners.Add(new ConsoleTraceListener(useErrorStream: true));
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Warning);
}
