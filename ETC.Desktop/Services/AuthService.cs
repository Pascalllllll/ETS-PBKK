using System.Text.RegularExpressions;
using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public record RegisterRequest(
    string Nama, UserRole Role, string NoPengenal, string? Jurusan, string NoTelp,
    IReadOnlyList<string> Spesialisasi, string Password, string ConfirmPassword);

public record ProfileUpdate(
    string Nama, string? Jurusan, string NoTelp, string? Bio, IReadOnlyList<string> Spesialisasi,
    int? Angkatan, string? Pengalaman, string? LinkedInUrl, string? PortofolioUrl, bool IsMentor);

public partial class AuthService(IDbContextFactory<EtcDbContext> factory, Session session)
{
    public async Task<Result> LoginAsync(string noPengenal, string password)
    {
        if (string.IsNullOrWhiteSpace(noPengenal) || string.IsNullOrEmpty(password))
            return Result.Fail("Isi NRP/NIP dan kata sandi.");

        await using var db = await factory.CreateDbContextAsync();
        var no = NormalizeNoPengenal(noPengenal);
        var user = await db.Users.FirstOrDefaultAsync(u => u.NoPengenal == no);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Result.Fail("NRP/NIP atau kata sandi tidak cocok.");

        session.SignIn(user);
        return Result.Success();
    }

    // Validator mengembalikan pesan galat, atau null bila valid.
    public static string? ValidateNama(string nama) =>
        string.IsNullOrWhiteSpace(nama) || nama.Trim().Length < 3 ? "Nama minimal 3 karakter." : null;

    public static string? ValidateIdentity(string nama, UserRole role, string noPengenal)
    {
        if (ValidateNama(nama) is { } err) return err;
        var digits = Digits(noPengenal);
        var onlyDigitsAndSpaces = noPengenal.Trim().All(c => char.IsDigit(c) || c == ' ');
        return role switch
        {
            UserRole.Mahasiswa when !onlyDigitsAndSpaces || digits.Length != 10 => "NRP terdiri dari 10 digit, contoh format 5025241XXX.",
            UserRole.Dosen when !onlyDigitsAndSpaces || digits.Length != 18 => "NIP terdiri dari 18 digit, contoh format 19810620 200501 1 003.",
            _ => null,
        };
    }

    // NIP disimpan dengan pengelompokan baku (8 6 1 3), jadi bisa diketik dengan atau tanpa spasi.
    public static string NormalizeNoPengenal(string noPengenal)
    {
        var d = Digits(noPengenal);
        return d.Length == 18 ? $"{d[..8]} {d[8..14]} {d[14]} {d[15..]}" : noPengenal.Trim();
    }

    static string Digits(string s) => new(s.Where(char.IsDigit).ToArray());

    public static string? ValidateContact(string noTelp)
    {
        if (!PhoneRegex().IsMatch(noTelp.Trim())) return "Nomor WhatsApp diawali 08 atau 62, 10 sampai 14 digit.";
        return null;
    }

    public static string? ValidatePassword(string password, string confirm)
    {
        if (password.Length < 8) return "Kata sandi minimal 8 karakter.";
        if (password != confirm) return "Konfirmasi kata sandi belum sama.";
        return null;
    }

    public async Task<Result> RegisterAsync(RegisterRequest r)
    {
        var err = ValidateIdentity(r.Nama, r.Role, r.NoPengenal) ?? ValidateContact(r.NoTelp)
                  ?? ValidatePassword(r.Password, r.ConfirmPassword);
        if (err is not null) return Result.Fail(err);

        await using var db = await factory.CreateDbContextAsync();
        var no = NormalizeNoPengenal(r.NoPengenal);
        if (await db.Users.AnyAsync(u => u.NoPengenal == no))
            return Result.Fail("NRP/NIP ini sudah terdaftar. Masuk dengan akun tersebut.");

        var user = new User
        {
            Nama = r.Nama.Trim(),
            Role = r.Role,
            NoPengenal = no,
            Jurusan = string.IsNullOrWhiteSpace(r.Jurusan) ? null : r.Jurusan,
            NoTelp = r.NoTelp.Trim(),
            Spesialisasi = r.Spesialisasi.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            Angkatan = r.Role == UserRole.Mahasiswa ? Seeder.AngkatanDariNrp(no) : null,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password, 10),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        session.SignIn(user);
        return Result.Success();
    }

    public static string? ValidateProfile(ProfileUpdate p)
    {
        if ((ValidateNama(p.Nama) ?? ValidateContact(p.NoTelp)) is { } err) return err;
        if (p.Angkatan is { } a && (a < 2000 || a > DateTime.Today.Year)) return $"Angkatan antara 2000 dan {DateTime.Today.Year}.";
        if (!string.IsNullOrWhiteSpace(p.LinkedInUrl) && !PendaftarService.IsUrl(p.LinkedInUrl))
            return "Tautan LinkedIn harus diawali https://";
        if (!string.IsNullOrWhiteSpace(p.PortofolioUrl) && !PendaftarService.IsUrl(p.PortofolioUrl))
            return "Tautan portofolio harus diawali https://";
        if (p.Pengalaman is { Length: > 1000 }) return "Pengalaman maksimal 1000 karakter.";
        if (p.IsMentor && string.IsNullOrWhiteSpace(p.Pengalaman))
            return "Tulis pengalaman atau prestasi Anda sebelum tampil sebagai mentor.";
        if (p.IsMentor && p.Spesialisasi.Count == 0) return "Tambahkan minimal satu keahlian sebelum tampil sebagai mentor.";
        return null;
    }

    public async Task<Result> UpdateProfileAsync(ProfileUpdate p)
    {
        if (ValidateProfile(p) is { } err) return Result.Fail(err);

        await using var db = await factory.CreateDbContextAsync();
        var user = await db.Users.FirstAsync(u => u.UserId == session.UserId);
        user.Nama = p.Nama.Trim();
        user.Jurusan = p.Jurusan;
        user.NoTelp = p.NoTelp.Trim();
        user.Bio = string.IsNullOrWhiteSpace(p.Bio) ? null : p.Bio.Trim();
        user.Spesialisasi = p.Spesialisasi.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        user.Angkatan = user.Role == UserRole.Mahasiswa ? p.Angkatan : null;
        user.Pengalaman = Clean(p.Pengalaman);
        user.LinkedInUrl = Clean(p.LinkedInUrl);
        user.PortofolioUrl = Clean(p.PortofolioUrl);
        user.IsMentor = p.IsMentor;
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        session.Refresh(user);
        return Result.Success();
    }

    static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    static readonly string[] FotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    // Foto disalin ke folder data aplikasi agar tetap ada walau berkas aslinya dipindah.
    public async Task<Result> SetFotoAsync(Stream source, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!FotoExtensions.Contains(ext)) return Result.Fail("Foto harus berformat JPG, PNG, atau WebP.");
        if (source.CanSeek && source.Length > 5 * 1024 * 1024) return Result.Fail("Ukuran foto maksimal 5 MB.");

        await using var db = await factory.CreateDbContextAsync();
        var user = await db.Users.FirstAsync(u => u.UserId == session.UserId);
        var dir = Path.Combine(AppPaths.DataDir, "foto");
        Directory.CreateDirectory(dir);
        // Nama berkas baru tiap unggah, supaya cache gambar tidak menampilkan foto lama.
        var path = Path.Combine(dir, $"{user.UserId:N}-{DateTime.Now:yyyyMMddHHmmss}{ext}");
        await using (var dest = File.Create(path)) await source.CopyToAsync(dest);

        var old = user.FotoPath;
        user.FotoPath = path;
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        if (old is not null && old != path) try { File.Delete(old); } catch (IOException) { }
        session.Refresh(user);
        return Result.Success();
    }

    public async Task<Result> RemoveFotoAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var user = await db.Users.FirstAsync(u => u.UserId == session.UserId);
        if (user.FotoPath is { } old) try { File.Delete(old); } catch (IOException) { }
        user.FotoPath = null;
        await db.SaveChangesAsync();
        session.Refresh(user);
        return Result.Success();
    }

    public void Logout() => session.SignOut();

    [GeneratedRegex(@"^(08|62)\d{8,12}$")]
    private static partial Regex PhoneRegex();
}
