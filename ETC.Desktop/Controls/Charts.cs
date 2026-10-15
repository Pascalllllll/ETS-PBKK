using Avalonia;
using Avalonia.Media;
using ETC.Desktop.Services;

namespace ETC.Desktop.Controls;

public class MonthlyStackedChart : ChartBase
{
    public static readonly StyledProperty<IReadOnlyList<MonthBucket>?> ItemsProperty =
        AvaloniaProperty.Register<MonthlyStackedChart, IReadOnlyList<MonthBucket>?>(nameof(Items));

    static MonthlyStackedChart() => AffectsRender<MonthlyStackedChart>(ItemsProperty);

    public IReadOnlyList<MonthBucket>? Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsProperty && Items is { Count: > 0 } items)
            SetSummary("Rekrutmen baru per bulan. " + string.Join(", ",
                items.Select(m => $"{m.FullLabel}: {m.Total} (lomba {m.Lomba}, riset {m.Riset}, projek {m.Projek})")));
    }

    public override void Render(DrawingContext ctx)
    {
        var items = Items;
        if (items is null || items.Count == 0) return;

        const double left = 26, bottom = 22, top = 14;
        var plot = new Rect(left, top, Math.Max(0, Bounds.Width - left), Math.Max(0, Bounds.Height - top - bottom));
        int max = Math.Max(4, items.Max(i => i.Total));
        int step = max <= 4 ? 1 : (int)Math.Ceiling(max / 4.0);
        int yMax = step * 4;

        var grid = new Pen(new SolidColorBrush(C("Chart.Grid")), 1);
        for (int g = 0; g <= 4; g++)
        {
            double y = Math.Round(plot.Bottom - plot.Height * g / 4) + 0.5;
            ctx.DrawLine(grid, new Point(plot.Left, y), new Point(plot.Right, y));
            var t = Text((g * step).ToString(), 11, "Chart.Text");
            ctx.DrawText(t, new Point(left - 8 - t.Width, y - t.Height / 2));
        }

        double slot = plot.Width / items.Count;
        double barW = Math.Min(30, slot * 0.58);
        int? hover = Pointer is { } p && plot.Inflate(new Thickness(0, top, 0, bottom)).Contains(p)
            ? Math.Clamp((int)((p.X - plot.Left) / slot), 0, items.Count - 1) : null;

        if (hover is { } hi)
            ctx.DrawRectangle(new SolidColorBrush(C("Chart.Grid"), 0.6), null,
                new Rect(plot.Left + slot * hi + 2, plot.Top - 6, slot - 4, plot.Height + 6), 6, 6);

        var colors = new[] { C("Chart.Projek"), C("Chart.Riset"), C("Chart.Lomba") };
        int maxIndex = Enumerable.Range(0, items.Count).OrderByDescending(i => items[i].Total).First();

        for (int i = 0; i < items.Count; i++)
        {
            var m = items[i];
            double x = plot.Left + slot * i + (slot - barW) / 2;
            double yBase = plot.Bottom;
            var values = new[] { m.Projek, m.Riset, m.Lomba };
            int lastNonZero = Array.FindLastIndex(values, v => v > 0);
            for (int s = 0; s < 3; s++)
            {
                if (values[s] == 0) continue;
                double h = plot.Height * values[s] / yMax;
                // Celah 2px agar batas segmen terbaca tanpa mengandalkan warna.
                double gap = s == lastNonZero ? 0 : 2;
                var rect = new Rect(x, yBase - h, barW, Math.Max(1, h - gap));
                var brush = new SolidColorBrush(colors[s]);
                if (s == lastNonZero) ctx.DrawGeometry(brush, null, TopRounded(rect, 4));
                else ctx.DrawRectangle(brush, null, rect);
                yBase -= h;
            }

            // Label langsung hanya di bulan tertinggi dan bulan berjalan.
            if ((i == maxIndex || i == items.Count - 1) && m.Total > 0)
            {
                var lbl = Text(m.Total.ToString(), 11, "Chart.TextStrong", bold: true);
                ctx.DrawText(lbl, new Point(x + barW / 2 - lbl.Width / 2, yBase - lbl.Height - 2));
            }

            var xl = Text(m.Label, 11, hover == i ? "Chart.TextStrong" : "Chart.Text", bold: hover == i);
            ctx.DrawText(xl, new Point(plot.Left + slot * i + slot / 2 - xl.Width / 2, plot.Bottom + 6));
        }

        if (hover is { } h2)
        {
            var m = items[h2];
            DrawTooltip(ctx, new Point(plot.Left + slot * h2 + slot / 2 + barW / 2, plot.Top + plot.Height / 2),
                m.FullLabel,
                [
                    (C("Chart.Lomba"), "Lomba", m.Lomba.ToString()),
                    (C("Chart.Riset"), "Riset", m.Riset.ToString()),
                    (C("Chart.Projek"), "Projek", m.Projek.ToString()),
                    (null, "Total", m.Total.ToString()),
                ]);
        }
    }
}

public class SkillDemandChart : ChartBase
{
    public static readonly StyledProperty<IReadOnlyList<SkillDemand>?> ItemsProperty =
        AvaloniaProperty.Register<SkillDemandChart, IReadOnlyList<SkillDemand>?>(nameof(Items));

    const double RowH = 28;

    static SkillDemandChart() => AffectsMeasure<SkillDemandChart>(ItemsProperty);

    public IReadOnlyList<SkillDemand>? Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }

    protected override Size MeasureOverride(Size availableSize) => new(0, (Items?.Count ?? 0) * RowH);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsProperty)
        {
            InvalidateVisual();
            if (Items is { } items)
                SetSummary("Keahlian paling dicari. " + string.Join(", ",
                    items.Select(s => $"{s.Skill}: {s.Count} rekrutmen{(s.Dimiliki ? ", Anda punya" : "")}")));
        }
    }

    public override void Render(DrawingContext ctx)
    {
        var items = Items;
        if (items is null || items.Count == 0) return;
        const double labelW = 128, valueW = 28;
        double trackW = Math.Max(10, Bounds.Width - labelW - valueW);
        int max = items.Max(i => i.Count);
        int? hover = Pointer is { } p ? (int)(p.Y / RowH) : null;
        if (hover >= items.Count) hover = null;
        var check = Icon("Icon.Check");

        for (int i = 0; i < items.Count; i++)
        {
            var s = items[i];
            double y = i * RowH;
            if (hover == i)
                ctx.DrawRectangle(new SolidColorBrush(C("Chart.Grid"), 0.6), null, new Rect(-6, y, Bounds.Width + 12, RowH), 6, 6);

            double lx = 0;
            if (s.Dimiliki && check is not null)
            {
                using (ctx.PushTransform(Matrix.CreateTranslation(0, 960) * Matrix.CreateScale(14 / 960.0, 14 / 960.0) *
                                         Matrix.CreateTranslation(0, y + 7)))
                    ctx.DrawGeometry(new SolidColorBrush(C("Chart.Success")), null, check);
                lx = 18;
            }
            var label = Text(s.Skill, 12, "Chart.TextStrong", bold: s.Dimiliki);
            label.MaxTextWidth = labelW - lx - 8;
            label.MaxLineCount = 1;
            label.Trimming = TextTrimming.CharacterEllipsis;
            ctx.DrawText(label, new Point(lx, y + (RowH - label.Height) / 2));

            double w = Math.Max(4, trackW * s.Count / max);
            var bar = new Rect(labelW, y + 8, w, RowH - 16);
            ctx.DrawGeometry(new SolidColorBrush(C("Chart.Projek")), null, RightRounded(bar, 4));

            var v = Text(s.Count.ToString(), 12, "Chart.TextStrong", bold: true);
            ctx.DrawText(v, new Point(labelW + w + 8, y + (RowH - v.Height) / 2));
        }

        if (hover is { } h)
        {
            var s = items[h];
            DrawTooltip(ctx, new Point(labelW + trackW * s.Count / max, h * RowH + RowH / 2), s.Skill,
            [
                (null, "Rekrutmen terbuka", s.Count.ToString()),
                (null, "Di profil Anda", s.Dimiliki ? "Ya" : "Belum"),
            ]);
        }
    }
}

public class StatusMeter : ChartBase
{
    public static readonly StyledProperty<StatusCount?> ValueProperty =
        AvaloniaProperty.Register<StatusMeter, StatusCount?>(nameof(Value));

    static StatusMeter() => AffectsRender<StatusMeter>(ValueProperty);

    public StatusCount? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    protected override Size MeasureOverride(Size availableSize) => new(0, 12);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty && Value is { } v)
            SetSummary($"{v.Menunggu} menunggu, {v.Diterima} diterima, {v.Ditolak} ditolak");
    }

    public override void Render(DrawingContext ctx)
    {
        var v = Value;
        var full = new Rect(0, 0, Bounds.Width, Bounds.Height);
        ctx.DrawRectangle(new SolidColorBrush(C("Chart.Track")), null, full, 4, 4);
        if (v is null || v.Total == 0) return;

        var parts = new (int n, string key, string label)[]
        {
            (v.Diterima, "Chart.Success", "Diterima"),
            (v.Menunggu, "Chart.Pending", "Menunggu"),
            (v.Ditolak, "Chart.Danger", "Ditolak"),
        }.Where(x => x.n > 0).ToArray();

        double x = 0;
        int? hover = null;
        var rects = new List<Rect>();
        for (int i = 0; i < parts.Length; i++)
        {
            double w = Bounds.Width * parts[i].n / v.Total;
            double gap = i == parts.Length - 1 ? 0 : 2;
            var r = new Rect(x, 0, Math.Max(2, w - gap), Bounds.Height);
            rects.Add(r);
            if (Pointer is { } p && p.X >= x && p.X < x + w) hover = i;
            x += w;
        }

        using (ctx.PushGeometryClip(new RectangleGeometry(full, 4, 4)))
            for (int i = 0; i < parts.Length; i++)
                ctx.DrawRectangle(new SolidColorBrush(C(parts[i].key)), null, rects[i]);

        if (hover is { } h)
            DrawTooltip(ctx, new Point(rects[h].Center.X, 6), parts[h].label,
                [(C(parts[h].key), "Jumlah", $"{parts[h].n} dari {v.Total}")]);
    }
}
