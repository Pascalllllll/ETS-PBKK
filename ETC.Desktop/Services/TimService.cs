using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public record TimMember(Guid UserId, string Nama, string? Jurusan, UserRole Role, int MemberKe,
    IReadOnlyList<string> Spesialisasi, string NoTelp, bool IsMe, int? RatingDariSaya);

public record TimSummary(Guid TimId, Guid RekrutmenId, string Judul, string Role, Kegiatan Kegiatan,
    DateTime TanggalMulai, DateTime TanggalSelesai, bool SayaKetua, IReadOnlyList<TimMember> Members)
{
    public bool Selesai => TanggalSelesai.Date < DateTime.Today;
    public bool Berjalan => !Selesai && TanggalMulai.Date <= DateTime.Today;
}

public class TimService(IDbContextFactory<EtcDbContext> factory, Session session)
{
    public async Task<List<TimSummary>> MyTeamsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        var tims = await db.Tims
            .Where(t => t.Participants.Any(p => p.UserId == me))
            .Include(t => t.Rekrutmen)
            .Include(t => t.Participants).ThenInclude(p => p.User)
            .Include(t => t.Histories)
            .AsSplitQuery()
            .ToListAsync();

        return tims
            // Tim yang baru berisi ketua belum dianggap tim.
            .Where(t => t.Participants.Count > 1)
            .Select(t => new TimSummary(
                t.TimId, t.RekrutmenId, t.Rekrutmen.Judul, t.Rekrutmen.Role, t.TipeTim,
                t.Rekrutmen.TanggalMulai, t.Rekrutmen.TanggalSelesai,
                t.Rekrutmen.UserId == me,
                t.Participants.OrderBy(p => p.MemberKe).Select(p => new TimMember(
                    p.UserId, p.User.Nama, p.User.Jurusan, p.User.Role, p.MemberKe, p.User.Spesialisasi, p.User.NoTelp,
                    p.UserId == me,
                    t.Histories.FirstOrDefault(h => h.UserId == p.UserId && h.ReviewerUserId == me)?.Rating)).ToList()))
            .OrderBy(t => t.Selesai).ThenByDescending(t => t.TanggalMulai)
            .ToList();
    }

    public static string? ValidateRating(int rating, string deskripsi)
    {
        if (rating is < 1 or > 5) return "Pilih 1 sampai 5 bintang.";
        if (string.IsNullOrWhiteSpace(deskripsi) || deskripsi.Trim().Length < 10) return "Tulis ulasan minimal 10 karakter.";
        return null;
    }

    public async Task<Result> RateAsync(Guid timId, Guid targetUserId, int rating, string deskripsi)
    {
        if (ValidateRating(rating, deskripsi) is { } err) return Result.Fail(err);
        var me = session.UserId;
        if (targetUserId == me) return Result.Fail("Anda tidak bisa menilai diri sendiri.");

        await using var db = await factory.CreateDbContextAsync();
        var members = await db.TimParticipants.Where(p => p.TimId == timId).Select(p => p.UserId).ToListAsync();
        if (!members.Contains(me) || !members.Contains(targetUserId))
            return Result.Fail("Ulasan hanya bisa diberikan ke rekan satu tim.");
        if (await db.Histories.AnyAsync(h => h.TimId == timId && h.UserId == targetUserId && h.ReviewerUserId == me))
            return Result.Fail("Anda sudah memberi ulasan untuk anggota ini.");

        db.Histories.Add(new History
        {
            TimId = timId, UserId = targetUserId, ReviewerUserId = me,
            Rating = rating, Deskripsi = deskripsi.Trim(),
        });
        await db.SaveChangesAsync();
        return Result.Success();
    }
}
