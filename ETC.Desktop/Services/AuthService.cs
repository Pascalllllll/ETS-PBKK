using System.Text.RegularExpressions;
using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public record RegisterRequest(
    string Nama, UserRole Role, string NoPengenal, string? Jurusan, string NoTelp,
    IReadOnlyList<string> Spesialisasi, string Password, string ConfirmPassword);

public record ProfileUpdate(string Nama, string? Jurusan, string NoTelp, string? Bio, IReadOnlyList<string> Spesialisasi);

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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password, 10),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        session.SignIn(user);
        return Result.Success();
    }

    public async Task<Result> UpdateProfileAsync(ProfileUpdate p)
    {
        var err = ValidateNama(p.Nama) ?? ValidateContact(p.NoTelp);
        if (err is not null) return Result.Fail(err);

        await using var db = await factory.CreateDbContextAsync();
        var user = await db.Users.FirstAsync(u => u.UserId == session.UserId);
        user.Nama = p.Nama.Trim();
        user.Jurusan = p.Jurusan;
        user.NoTelp = p.NoTelp.Trim();
        user.Bio = string.IsNullOrWhiteSpace(p.Bio) ? null : p.Bio.Trim();
        user.Spesialisasi = p.Spesialisasi.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        session.Refresh(user);
        return Result.Success();
    }

    public void Logout() => session.SignOut();

    [GeneratedRegex(@"^(08|62)\d{8,12}$")]
    private static partial Regex PhoneRegex();
}
