using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace ETC.Desktop.Controls;

public abstract class ChartBase : Control
{
    protected Point? Pointer;
    static Typeface? _typeface, _typefaceBold;

    protected ChartBase()
    {
        ClipToBounds = false;
        Focusable = false;
        ActualThemeVariantChanged += (_, _) => InvalidateVisual();
    }

    protected Typeface Regular => _typeface ??= new Typeface(Font(), FontStyle.Normal, FontWeight.Medium);
    protected Typeface Bold => _typefaceBold ??= new Typeface(Font(), FontStyle.Normal, FontWeight.Bold);

    FontFamily Font() =>
        Application.Current!.TryFindResource("Font.Ui", out var f) && f is FontFamily ff ? ff : FontFamily.Default;

    protected Color C(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var v) && v is Color c ? c : Colors.Gray;

    protected IBrush B(string key, double opacity = 1) => new SolidColorBrush(C(key), opacity);

    protected Geometry? Icon(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var v) ? v as Geometry : null;

    protected FormattedText Text(string s, double size, string colorKey, bool bold = false) =>
        new(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, bold ? Bold : Regular, size, B(colorKey));

    protected void SetSummary(string summary) => AutomationProperties.SetName(this, summary);

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        Pointer = e.GetPosition(this);
        InvalidateVisual();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Pointer = null;
        InvalidateVisual();
    }

    protected static Geometry TopRounded(Rect r, double radius)
    {
        radius = Math.Min(radius, Math.Min(r.Width / 2, r.Height));
        var g = new StreamGeometry();
        using var c = g.Open();
        c.BeginFigure(r.BottomLeft, true);
        c.LineTo(new Point(r.Left, r.Top + radius));
        c.ArcTo(new Point(r.Left + radius, r.Top), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        c.LineTo(new Point(r.Right - radius, r.Top));
        c.ArcTo(new Point(r.Right, r.Top + radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        c.LineTo(r.BottomRight);
        c.EndFigure(true);
        return g;
    }

    protected static Geometry RightRounded(Rect r, double radius)
    {
        radius = Math.Min(radius, Math.Min(r.Height / 2, r.Width));
        var g = new StreamGeometry();
        using var c = g.Open();
        c.BeginFigure(r.TopLeft, true);
        c.LineTo(new Point(r.Right - radius, r.Top));
        c.ArcTo(new Point(r.Right, r.Top + radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        c.LineTo(new Point(r.Right, r.Bottom - radius));
        c.ArcTo(new Point(r.Right - radius, r.Bottom), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        c.LineTo(r.BottomLeft);
        c.EndFigure(true);
        return g;
    }

    protected void DrawTooltip(DrawingContext ctx, Point anchor, string title,
        IReadOnlyList<(Color? swatch, string label, string value)> rows)
    {
        const double pad = 10, rowH = 18, sw = 8;
        var titleText = Text(title, 12, "Chart.TooltipText", bold: true);
        var labels = rows.Select(r => Text(r.label, 12, "Chart.TooltipText")).ToList();
        var values = rows.Select(r => Text(r.value, 12, "Chart.TooltipText", bold: true)).ToList();
        double labelW = labels.Count == 0 ? 0 : labels.Max(l => l.Width);
        double valueW = values.Count == 0 ? 0 : values.Max(v => v.Width);
        double w = Math.Max(titleText.Width, sw + 6 + labelW + 16 + valueW) + pad * 2;
        double h = pad * 2 + titleText.Height + 4 + rows.Count * rowH;

        double x = anchor.X + 12;
        if (x + w > Bounds.Width) x = anchor.X - w - 12;
        x = Math.Max(0, x);
        double y = Math.Clamp(anchor.Y - h / 2, -8, Math.Max(-8, Bounds.Height - h + 8));

        var bg = new SolidColorBrush(C("Chart.Tooltip"));
        ctx.DrawRectangle(bg, null, new Rect(x, y, w, h), 8, 8);
        ctx.DrawText(titleText, new Point(x + pad, y + pad));
        double ry = y + pad + titleText.Height + 4;
        for (int i = 0; i < rows.Count; i++)
        {
            double tx = x + pad;
            if (rows[i].swatch is { } col)
            {
                ctx.DrawRectangle(new SolidColorBrush(col), new Pen(new SolidColorBrush(C("Chart.Tooltip")), 1),
                    new Rect(tx, ry + 5, sw, sw), 2, 2);
                tx += sw + 6;
            }
            ctx.DrawText(labels[i], new Point(tx, ry + 1));
            ctx.DrawText(values[i], new Point(x + w - pad - values[i].Width, ry + 1));
            ry += rowH;
        }
    }
}
