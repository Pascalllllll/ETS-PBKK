using ETC.Desktop.Models;

namespace ETC.Desktop.Services;

public record Result(bool Ok, string? Error = null)
{
    public static Result Success() => new(true);
    public static Result Fail(string error) => new(false, error);
}

public record Result<T>(bool Ok, T? Value, string? Error = null)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Fail(string error) => new(false, default, error);
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
}

public class Session
{
    public User? CurrentUser { get; private set; }
    public event Action? Changed;

    public Guid UserId => CurrentUser?.UserId ?? Guid.Empty;

    public void SignIn(User user) { CurrentUser = user; Changed?.Invoke(); }
    public void SignOut() { CurrentUser = null; Changed?.Invoke(); }
    public void Refresh(User user) { CurrentUser = user; Changed?.Invoke(); }
}

public static class Labels
{
    public static string Of(Kegiatan k) => k switch
    {
        Kegiatan.Lomba => "Lomba",
        Kegiatan.Riset => "Riset",
        _ => "Projek",
    };

    public static string Of(StatusPendaftar s) => s switch
    {
        StatusPendaftar.Accepted => "Diterima",
        StatusPendaftar.Rejected => "Ditolak",
        _ => "Menunggu",
    };

    public static string Of(StatusBimbingan s) => s switch
    {
        StatusBimbingan.Accepted => "Diterima",
        StatusBimbingan.Rejected => "Ditolak",
        _ => "Menunggu",
    };

    public static readonly string[] Jurusan =
    {
        "Teknik Informatika", "Rekayasa Perangkat Lunak", "Rekayasa Kecerdasan Artifisial", "Sistem Informasi",
        "Teknologi Informasi", "Teknik Komputer", "Teknik Elektro", "Matematika", "Statistika",
    };
}
