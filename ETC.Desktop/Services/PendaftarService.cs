using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public record ApplyInput(string? Alasan, string CvUrl, string PortofolioUrl);

public record ApplicantSummary(
    Guid PendaftarId, Guid UserId, string Nama, string? Jurusan, string NoPengenal, string NoTelp,
    IReadOnlyList<string> Spesialisasi, string? Alasan, string CvUrl, string PortofolioUrl,
    StatusPendaftar Status, DateTime CreatedAt, double? Rating, int JumlahUlasan, int TimSelesai);

public record ReviewItem(string ReviewerNama, string Kegiatan, int Rating, string Deskripsi, DateTime CreatedAt);

public record MyApplication(Guid PendaftarId, StatusPendaftar Status, DateTime CreatedAt, DateTime? DecidedAt, RekrutmenSummary Rekrutmen);

public class PendaftarService(IDbContextFactory<EtcDbContext> factory, Session session)
{
    public static string? Validate(ApplyInput i)
    {
        if (!IsUrl(i.CvUrl)) return "Tautan CV harus diawali https://";
        if (!IsUrl(i.PortofolioUrl)) return "Tautan portofolio harus diawali https://";
        if (i.Alasan is { Length: > 500 }) return "Alasan maksimal 500 karakter.";
        return null;
    }

    internal static bool IsUrl(string? s) =>
        Uri.TryCreate(s?.Trim(), UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp);

    public async Task<Result> ApplyAsync(Guid rekrutmenId, ApplyInput input)
    {
        if (Validate(input) is { } err) return Result.Fail(err);
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        var r = await db.Rekrutmens.FirstOrDefaultAsync(x => x.RekrutmenId == rekrutmenId);
        if (r is null) return Result.Fail("Rekrutmen tidak ditemukan.");
        if (r.UserId == me) return Result.Fail("Anda tidak bisa mendaftar ke rekrutmen sendiri.");
        if (!r.IsOpen || r.BatasPendaftaran.Date < DateTime.Today) return Result.Fail("Pendaftaran untuk rekrutmen ini sudah ditutup.");
        if (await db.Pendaftars.AnyAsync(p => p.RekrutmenId == rekrutmenId && p.UserId == me))
            return Result.Fail("Anda sudah mendaftar di rekrutmen ini.");

        db.Pendaftars.Add(new Pendaftar
        {
            RekrutmenId = rekrutmenId,
            UserId = me,
            AlasanMendaftar = string.IsNullOrWhiteSpace(input.Alasan) ? null : input.Alasan.Trim(),
            CvUrl = input.CvUrl.Trim(),
            PortofolioUrl = input.PortofolioUrl.Trim(),
        });
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> WithdrawAsync(Guid pendaftarId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var p = await db.Pendaftars.FirstOrDefaultAsync(x => x.PendaftarId == pendaftarId && x.UserId == session.UserId);
        if (p is null) return Result.Fail("Lamaran tidak ditemukan.");
        if (p.Status != StatusPendaftar.Pending) return Result.Fail("Lamaran yang sudah diputuskan tidak bisa dibatalkan.");
        db.Pendaftars.Remove(p);
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<List<MyApplication>> MyApplicationsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        var apps = await db.Pendaftars.Where(p => p.UserId == me)
            .Select(p => new { p.PendaftarId, p.Status, p.CreatedAt, p.DecidedAt, p.RekrutmenId })
            .ToListAsync();
        var ids = apps.Select(a => a.RekrutmenId).ToList();
        var reks = await RekrutmenService.Project(db.Rekrutmens.Where(r => ids.Contains(r.RekrutmenId)), me)
            .ToDictionaryAsync(r => r.Id);
        return apps.Select(a => new MyApplication(a.PendaftarId, a.Status, a.CreatedAt, a.DecidedAt, reks[a.RekrutmenId]))
            .OrderBy(a => a.Status != StatusPendaftar.Pending).ThenByDescending(a => a.CreatedAt).ToList();
    }

    public async Task<List<ApplicantSummary>> ApplicantsAsync(Guid rekrutmenId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var owner = await db.Rekrutmens.Where(r => r.RekrutmenId == rekrutmenId).Select(r => r.UserId).FirstOrDefaultAsync();
        if (owner != session.UserId) return new();

        var list = await db.Pendaftars.Where(p => p.RekrutmenId == rekrutmenId)
            .Select(p => new ApplicantSummary(
                p.PendaftarId, p.UserId, p.User.Nama, p.User.Jurusan, p.User.NoPengenal, p.User.NoTelp,
                p.User.Spesialisasi, p.AlasanMendaftar, p.CvUrl, p.PortofolioUrl, p.Status, p.CreatedAt,
                p.User.UlasanDiterima.Select(h => (double?)h.Rating).Average(),
                p.User.UlasanDiterima.Count,
                p.User.TimParticipants.Count(t => t.Tim.Rekrutmen.TanggalSelesai < DateTime.Today)))
            .ToListAsync();
        return list.OrderBy(a => a.Status).ThenBy(a => a.CreatedAt).ToList();
    }

    public async Task<List<ReviewItem>> ReviewsForAsync(Guid userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var rows = await db.Histories.Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new { h.Reviewer.Nama, h.Tim.Rekrutmen.Judul, h.Rating, h.Deskripsi, h.CreatedAt })
            .ToListAsync();
        return rows.Select(h => new ReviewItem(h.Nama, h.Judul, h.Rating, h.Deskripsi, h.CreatedAt)).ToList();
    }

    public async Task<Result> DecideAsync(Guid pendaftarId, bool accept)
    {
        await using var db = await factory.CreateDbContextAsync();
        var p = await db.Pendaftars.Include(x => x.Rekrutmen).ThenInclude(r => r.Tim!).ThenInclude(t => t.Participants)
            .FirstOrDefaultAsync(x => x.PendaftarId == pendaftarId);
        if (p is null) return Result.Fail("Pelamar tidak ditemukan.");
        if (p.Rekrutmen.UserId != session.UserId) return Result.Fail("Hanya pembuat rekrutmen yang bisa memutuskan.");
        if (p.Status != StatusPendaftar.Pending) return Result.Fail("Pelamar ini sudah diputuskan.");

        var r = p.Rekrutmen;
        if (accept)
        {
            var accepted = await db.Pendaftars.CountAsync(x => x.RekrutmenId == r.RekrutmenId && x.Status == StatusPendaftar.Accepted);
            if (accepted >= r.Kuota) return Result.Fail("Kuota sudah penuh. Naikkan kuota untuk menerima pelamar lagi.");

            var tim = r.Tim ?? new Tim { RekrutmenId = r.RekrutmenId, TipeTim = r.Kegiatan };
            if (r.Tim is null)
            {
                tim.Participants.Add(new TimParticipant { TimId = tim.TimId, UserId = r.UserId, MemberKe = 1 });
                db.Tims.Add(tim);
            }
            if (tim.Participants.All(t => t.UserId != p.UserId))
                db.TimParticipants.Add(new TimParticipant
                {
                    TimId = tim.TimId,
                    UserId = p.UserId,
                    MemberKe = tim.Participants.Count == 0 ? 2 : tim.Participants.Max(t => t.MemberKe) + 1,
                });
            if (accepted + 1 >= r.Kuota) r.IsOpen = false;
        }

        p.Status = accept ? StatusPendaftar.Accepted : StatusPendaftar.Rejected;
        p.DecidedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Result.Success();
    }
}
