using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ETC.Desktop.Services;
using ETC.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ETC.Desktop;

// ETC_SNAPSHOT_DIR: render tiap halaman ke PNG (terang dan gelap) untuk dokumentasi, lalu keluar.
internal static class Snapshot
{
    public static string? Dir => Environment.GetEnvironmentVariable("ETC_SNAPSHOT_DIR");

    public static async Task RunAsync(Window window, IServiceProvider sp)
    {
        var dir = Dir!;
        Directory.CreateDirectory(dir);
        var user = Environment.GetEnvironmentVariable("ETC_SNAPSHOT_USER") ?? "5025241177";
        var pages = (Environment.GetEnvironmentVariable("ETC_SNAPSHOT_PAGES") ?? "auth,Ringkasan,Jelajahi,Tersimpan,RekrutmenSaya,LamaranSaya,TimSaya,Mentor,Profil")
            .Split(',', StringSplitOptions.RemoveEmptyEntries);
        var themes = (Environment.GetEnvironmentVariable("ETC_SNAPSHOT_THEMES") ?? "light,dark").Split(',');

        foreach (var theme in themes)
        {
            Application.Current!.RequestedThemeVariant = theme == "dark" ? ThemeVariant.Dark : ThemeVariant.Light;
            foreach (var page in pages)
            {
                if (page == "auth")
                {
                    sp.GetRequiredService<AuthService>().Logout();
                }
                else
                {
                    if (sp.GetRequiredService<Session>().CurrentUser is null)
                        await sp.GetRequiredService<AuthService>().LoginAsync(user, Data.Seeder.DemoPassword);
                    if (sp.GetRequiredService<MainViewModel>().Current is ShellViewModel shell)
                    {
                        if (page.StartsWith("sheet:"))
                        {
                            shell.Navigate(Page.RekrutmenSaya);
                            await Task.Delay(600);
                            shell.NewRekrutmenCommand.Execute(null);
                        }
                        else shell.Navigate(Enum.Parse<Page>(page));
                    }
                }
                await Task.Delay(1500);
                await Dispatcher.UIThread.InvokeAsync(() => Render(window, Path.Combine(dir, $"{theme}-{page.Replace(':', '-')}.png")));
                var scroller = window.GetVisualDescendants().OfType<ScrollViewer>()
                    .Where(v => v.IsEffectivelyVisible && v.Extent.Height > v.Viewport.Height + 40)
                    .OrderByDescending(v => v.Extent.Height).FirstOrDefault();
                if (scroller is not null && page != "auth")
                {
                    scroller.Offset = new Vector(0, scroller.Extent.Height);
                    await Task.Delay(400);
                    await Dispatcher.UIThread.InvokeAsync(() => Render(window, Path.Combine(dir, $"{theme}-{page.Replace(':', '-')}-bawah.png")));
                }
                if (sp.GetRequiredService<MainViewModel>().Current is ShellViewModel s2 && s2.HasSheet) s2.CloseSheetCommand.Execute(null);
            }
        }
        window.Close();
    }

    static void Render(Window window, string path)
    {
        var size = new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height);
        using var bmp = new RenderTargetBitmap(size, new Vector(96, 96));
        bmp.Render(window);
        bmp.Save(path);
    }
}
