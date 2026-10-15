using System.Globalization;
using ETC.Desktop.Models;

namespace ETC.Desktop.ViewModels;

public static class Format
{
    public static readonly CultureInfo Id = CultureInfo.GetCultureInfo("id-ID");

    public static string Date(DateTime d) => d.ToString("d MMM yyyy", Id);
    public static string DateShort(DateTime d) => d.ToString("d MMM", Id);
    public static string Range(DateTime a, DateTime b) =>
        a.Year == b.Year ? $"{a.ToString("d MMM", Id)} – {b.ToString("d MMM yyyy", Id)}" : $"{Date(a)} – {Date(b)}";

    public static string Money(decimal? fee) =>
        fee is null or 0 ? "Tanpa fee" : "Rp" + fee.Value.ToString("#,0", Id);

    public static string Deadline(DateTime batas, DateTime? today = null)
    {
        var days = (batas.Date - (today ?? DateTime.Today)).Days;
        return days switch
        {
            < 0 => $"Ditutup {DateShort(batas)}",
            0 => "Tutup hari ini",
            1 => "Tutup besok",
            <= 14 => $"Tutup {days} hari lagi",
            _ => $"Tutup {DateShort(batas)}",
        };
    }

    public static bool Urgent(DateTime batas) => (batas.Date - DateTime.Today).Days is >= 0 and <= 3;

    public static string Ago(DateTime t)
    {
        var span = DateTime.Now - t;
        if (span.TotalMinutes < 1) return "baru saja";
        if (span.TotalHours < 1) return $"{(int)span.TotalMinutes} menit lalu";
        if (span.TotalDays < 1) return $"{(int)span.TotalHours} jam lalu";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays} hari lalu";
        return Date(t);
    }

    public static string Failure(string what, Exception ex)
    {
        System.Diagnostics.Trace.TraceError(ex.ToString());
        return $"{what} tidak bisa dimuat. Coba muat ulang; bila tetap gagal, tutup lalu buka kembali aplikasi.";
    }

    public static string DaysLeft(DateTime d)
    {
        var days = (d.Date - DateTime.Today).Days;
        return days switch { < 0 => "Sudah lewat", 0 => "Hari ini", 1 => "Besok", _ => $"{days} hari lagi" };
    }

    public static string WhatsAppUrl(string noTelp) =>
        "https://wa.me/" + (noTelp.StartsWith('0') ? "62" + noTelp[1..] : noTelp);

    // Tanpa gelar: "Prof. Ir. Ary Mazharuddin Shiddiqi, S.Kom., ..." menjadi "Ary Mazharuddin Shiddiqi".
    public static string NameWithoutTitles(string nama) =>
        string.Join(' ', nama.Split(',')[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).SkipWhile(p => p.EndsWith('.')));

    public static string Initials(string nama)
    {
        var parts = NameWithoutTitles(nama).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpper(),
            _ => (parts[0][..1] + parts[1][..1]).ToUpper(),
        };
    }

    public static string KegiatanIcon(Kegiatan k) => k switch
    {
        Kegiatan.Lomba => "Icon.TrophyFill",
        Kegiatan.Riset => "Icon.ScienceFill",
        _ => "Icon.CodeFill",
    };

    public static string Rating(double? r, int n) => r is null ? "Belum ada ulasan" : $"{r:0.0} dari {n} ulasan";

    public static string Greeting(DateTime now) => now.Hour switch
    {
        < 11 => "Selamat pagi",
        < 15 => "Selamat siang",
        < 18 => "Selamat sore",
        _ => "Selamat malam",
    };

    public static string FirstName(string nama) => NameWithoutTitles(nama).Split(' ').FirstOrDefault() ?? nama;
}
