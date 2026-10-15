using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ETC.Desktop.Controls;

// Digambar pada kotak 960x960 asli agar ukuran semua ikon seragam; PathIcon meregangkan tiap path.
public class Glyph : Control
{
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<Glyph, Geometry?>(nameof(Data));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<Glyph>();

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Glyph, double>(nameof(Size), 18);

    static Glyph()
    {
        AffectsRender<Glyph>(DataProperty, ForegroundProperty);
        AffectsMeasure<Glyph>(SizeProperty);
    }

    public Geometry? Data { get => GetValue(DataProperty); set => SetValue(DataProperty, value); }
    public IBrush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    protected override Size MeasureOverride(Size availableSize) => new(Size, Size);

    public override void Render(DrawingContext context)
    {
        if (Data is null) return;
        var s = Size / 960.0;
        using (context.PushTransform(Matrix.CreateTranslation(0, 960) * Matrix.CreateScale(s, s)))
            context.DrawGeometry(Foreground ?? Brushes.Gray, null, Data);
    }
}
