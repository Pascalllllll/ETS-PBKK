using System.Text.Json;
using Avalonia;
using Avalonia.Styling;

namespace ETC.Desktop.Services;

public enum Appearance { System, Light, Dark }

public class AppSettings
{
    record Stored(Appearance Appearance);

    readonly string _path = Path.Combine(AppPaths.DataDir, "settings.json");
    public Appearance Appearance { get; private set; }

    public AppSettings()
    {
        try
        {
            if (File.Exists(_path))
                Appearance = JsonSerializer.Deserialize<Stored>(File.ReadAllText(_path))?.Appearance ?? Appearance.System;
        }
        catch (Exception) { Appearance = Appearance.System; }
    }

    public void SetAppearance(Appearance a)
    {
        Appearance = a;
        Apply();
        try { File.WriteAllText(_path, JsonSerializer.Serialize(new Stored(a))); }
        catch (IOException) { }
    }

    public void Apply()
    {
        if (Application.Current is { } app)
            app.RequestedThemeVariant = Appearance switch
            {
                Appearance.Light => ThemeVariant.Light,
                Appearance.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
    }
}

public static class AppPaths
{
    public static string DataDir
    {
        get
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ETC");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string Database => Environment.GetEnvironmentVariable("ETC_DB") ?? Path.Combine(DataDir, "etc.db");
}
