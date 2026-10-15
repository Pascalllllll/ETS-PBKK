using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using ETC.Desktop.Data;
using ETC.Desktop.Services;
using ETC.Desktop.ViewModels;
using ETC.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ETC.Desktop;

public class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public static ServiceProvider BuildServices(string connectionString)
    {
        var s = new ServiceCollection();
        s.AddDbContextFactory<EtcDbContext>(o => o.UseSqlite(connectionString));
        s.AddSingleton<Session>();
        s.AddSingleton<AppSettings>();
        s.AddTransient<AuthService>();
        s.AddTransient<RekrutmenService>();
        s.AddTransient<PendaftarService>();
        s.AddTransient<TimService>();
        s.AddTransient<DashboardService>();
        s.AddSingleton<MainViewModel>();
        return s.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = BuildServices($"Data Source={AppPaths.Database}");

        using (var db = Services.GetRequiredService<IDbContextFactory<EtcDbContext>>().CreateDbContext())
        {
            db.Database.EnsureCreated();
            Seeder.Seed(db);
        }
        Services.GetRequiredService<AppSettings>().Apply();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow { DataContext = Services.GetRequiredService<MainViewModel>() };
            if (int.TryParse(Environment.GetEnvironmentVariable("ETC_SNAPSHOT_HEIGHT"), out var h)) window.Height = h;
            desktop.MainWindow = window;
            if (Snapshot.Dir is not null)
                window.Opened += async (_, _) => await Snapshot.RunAsync(window, Services);
        }

        base.OnFrameworkInitializationCompleted();
    }
}

// FooViewModel ditampilkan oleh FooView.
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null) return null;
        var name = param.GetType().FullName!.Replace("ViewModels", "Views").Replace("ViewModel", "View");
        var type = Type.GetType(name);
        return type is null ? new TextBlock { Text = "View tidak ditemukan: " + name } : (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object? data) => data is ViewModelBase;
}
