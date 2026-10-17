using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public record MentorSummary(
    Guid UserId, string Nama, UserRole Role, string? Jurusan, int? Angkatan, IReadOnlyList<string> Skills,
    string Pengalaman, string? LinkedInUrl, string? PortofolioUrl, string? FotoPath, string NoTelp,
    double? Rating, int JumlahUlasan, int Dibimbing);

public record MentorRequestInput(Guid MentorId, Kegiatan Kegiatan, string Topik, string Pesan);

public record MentorRequestItem(
    Guid Id, Guid MentorId, string MentorNama, string? MentorFoto, Guid UserId, string UserNama, string? UserFoto,
    string? UserJurusan, string UserNoTelp, string MentorNoTelp, Kegiatan Kegiatan, string Topik, string Pesan,
    StatusBimbingan Status, DateTime CreatedAt, DateTime? DecidedAt);

public class MentorService(IDbContextFactory<EtcDbContext> factory, Session session)
{
    public async Task<List<MentorSummary>> ListAsync(string? text = null, UserRole? role = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        IQueryable<User> q = db.Users.Where(u => u.IsMentor && u.UserId != me);
        if (role is { } r) q = q.Where(u => u.Role == r);

        var mentors = await q.Select(u => new
        {
            u.UserId, u.Nama, u.Role, u.Jurusan, u.Angkatan, u.Spesialisasi, u.Pengalaman, u.LinkedInUrl,
            u.PortofolioUrl, u.FotoPath, u.NoTelp,
            Ratings = u.UlasanDiterima.Select(h => h.Rating).ToList(),
        }).ToListAsync();
        var dibimbing = await db.MentorRequests.Where(m => m.Status == StatusBimbingan.Accepted)
            .GroupBy(m => m.MentorId).Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N);

        // Keahlian disimpan sebagai JSON, jadi pencarian teks dilakukan di memori.
        var t = text?.Trim() ?? "";
        return mentors
            .Where(m => t.Length == 0 ||
                        m.Nama.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                        (m.Pengalaman?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        m.Spesialisasi.Any(s => s.Contains(t, StringComparison.OrdinalIgnoreCase)))
            .Select(m => new MentorSummary(m.UserId, m.Nama, m.Role, m.Jurusan, m.Angkatan, m.Spesialisasi,
                m.Pengalaman ?? "", m.LinkedInUrl, m.PortofolioUrl, m.FotoPath, m.NoTelp,
                m.Ratings.Count > 0 ? m.Ratings.Average() : null, m.Ratings.Count, dibimbing.GetValueOrDefault(m.UserId)))
            .OrderBy(m => m.Role == UserRole.Dosen ? 0 : 1)
            .ThenByDescending(m => m.Dibimbing)
            .ThenBy(m => m.Nama)
            .ToList();
    }

    public static string? Validate(MentorRequestInput i)
    {
        if (i.MentorId == Guid.Empty) return "Pilih mentor yang ingin dihubungi.";
        if (string.IsNullOrWhiteSpace(i.Topik) || i.Topik.Trim().Length < 5) return "Tulis topik bimbingan, minimal 5 karakter.";
        if (i.Topik.Length > 120) return "Topik maksimal 120 karakter.";
        if (string.IsNullOrWhiteSpace(i.Pesan) || i.Pesan.Trim().Length < 20)
            return "Ceritakan kebutuhan Anda minimal 20 karakter, supaya mentor bisa menilai.";
        if (i.Pesan.Length > 800) return "Pesan maksimal 800 karakter.";
        return null;
    }

    public async Task<Result> RequestAsync(MentorRequestInput i)
    {
        if (Validate(i) is { } err) return Result.Fail(err);
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        if (i.MentorId == me) return Result.Fail("Anda tidak bisa meminta bimbingan dari diri sendiri.");
        if (!await db.Users.AnyAsync(u => u.UserId == i.MentorId && u.IsMentor))
            return Result.Fail("Pengguna ini sedang tidak membuka bimbingan.");
        if (await db.MentorRequests.AnyAsync(m => m.MentorId == i.MentorId && m.UserId == me && m.Status == StatusBimbingan.Pending))
            return Result.Fail("Permintaan Anda ke mentor ini masih menunggu jawaban.");

        db.MentorRequests.Add(new MentorRequest
        {
            MentorId = i.MentorId,
            UserId = me,
            Kegiatan = i.Kegiatan,
            Topik = i.Topik.Trim(),
            Pesan = i.Pesan.Trim(),
        });
        await db.SaveChangesAsync();
        return Result.Success();
    }

    static IQueryable<MentorRequestItem> Project(IQueryable<MentorRequest> q) =>
        q.Select(m => new MentorRequestItem(m.Id, m.MentorId, m.Mentor.Nama, m.Mentor.FotoPath, m.UserId, m.User.Nama,
            m.User.FotoPath, m.User.Jurusan, m.User.NoTelp, m.Mentor.NoTelp, m.Kegiatan, m.Topik, m.Pesan, m.Status,
            m.CreatedAt, m.DecidedAt));

    public async Task<List<MentorRequestItem>> SentAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var list = await Project(db.MentorRequests.Where(m => m.UserId == session.UserId)).ToListAsync();
        return list.OrderByDescending(m => m.CreatedAt).ToList();
    }

    // Menunggu jawaban ditampilkan lebih dulu, yang terlama di atas.
    public async Task<List<MentorRequestItem>> IncomingAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var list = await Project(db.MentorRequests.Where(m => m.MentorId == session.UserId)).ToListAsync();
        return list.OrderBy(m => m.Status == StatusBimbingan.Pending ? 0 : 1)
            .ThenBy(m => m.Status == StatusBimbingan.Pending ? m.CreatedAt.Ticks : -m.CreatedAt.Ticks).ToList();
    }

    public async Task<Result> DecideAsync(Guid requestId, bool accept)
    {
        await using var db = await factory.CreateDbContextAsync();
        var m = await db.MentorRequests.FirstOrDefaultAsync(x => x.Id == requestId);
        if (m is null || m.MentorId != session.UserId) return Result.Fail("Permintaan ini bukan untuk Anda.");
        if (m.Status != StatusBimbingan.Pending) return Result.Fail("Permintaan ini sudah dijawab.");
        m.Status = accept ? StatusBimbingan.Accepted : StatusBimbingan.Rejected;
        m.DecidedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid requestId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var m = await db.MentorRequests.FirstOrDefaultAsync(x => x.Id == requestId && x.UserId == session.UserId);
        if (m is null) return Result.Fail("Permintaan tidak ditemukan.");
        if (m.Status != StatusBimbingan.Pending) return Result.Fail("Permintaan yang sudah dijawab tidak bisa dibatalkan.");
        db.MentorRequests.Remove(m);
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<int> PendingIncomingCountAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.MentorRequests.CountAsync(m => m.MentorId == session.UserId && m.Status == StatusBimbingan.Pending);
    }
}
