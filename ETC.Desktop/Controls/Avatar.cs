using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ETC.Desktop.ViewModels;

namespace ETC.Desktop.Controls;

// Foto profil bila ada, selain itu inisial nama.
public class Avatar : Border
{
    public static readonly StyledProperty<string?> NamaProperty = AvaloniaProperty.Register<Avatar, string?>(nameof(Nama));
    public static readonly StyledProperty<string?> FotoPathProperty = AvaloniaProperty.Register<Avatar, string?>(nameof(FotoPath));
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<Avatar, double>(nameof(Size), 36);

    static readonly Dictionary<string, Bitmap?> Cache = new();

    readonly TextBlock _initials = new()
    {
        FontWeight = FontWeight.Bold,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
    };
    readonly Image _image = new() { Stretch = Stretch.UniformToFill };

    public string? Nama { get => GetValue(NamaProperty); set => SetValue(NamaProperty, value); }
    public string? FotoPath { get => GetValue(FotoPathProperty); set => SetValue(FotoPathProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    public Avatar()
    {
        ClipToBounds = true;
        _initials.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable("Selection.Fg"));
        this.Bind(BackgroundProperty, this.GetResourceObservable("Selection.Bg"));
        Child = new Panel { Children = { _initials, _image } };
        Update();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == NamaProperty || change.Property == FotoPathProperty || change.Property == SizeProperty) Update();
    }

    void Update()
    {
        Width = Height = Size;
        CornerRadius = new CornerRadius(Size / 2);
        _initials.FontSize = Math.Round(Size * 0.36);
        _initials.Text = Format.Initials(Nama ?? "");
        var bmp = Load(FotoPath);
        _image.Source = bmp;
        _image.IsVisible = bmp is not null;
        _initials.IsVisible = bmp is null;
    }

    static Bitmap? Load(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (Cache.TryGetValue(path, out var cached)) return cached;
        Bitmap? bmp = null;
        try
        {
            if (File.Exists(path))
            {
                using var s = File.OpenRead(path);
                bmp = Bitmap.DecodeToWidth(s, 256);
            }
        }
        catch (Exception) { bmp = null; }
        return Cache[path] = bmp;
    }
}
