using System.Globalization;
using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public record MonthBucket(string Label, string FullLabel, int Lomba, int Riset, int Projek)
{
    public int Total => Lomba + Riset + Projek;
}

public record SkillDemand(string Skill, int Count, bool Dimiliki);

public record StatusCount(int Menunggu, int Diterima, int Ditolak)
{
    public int Total => Menunggu + Diterima + Ditolak;
}

public record PendingApplicant(Guid PendaftarId, Guid RekrutmenId, string Nama, string? Jurusan, string Judul,
    IReadOnlyList<string> SkillCocok, int SkillDiminta, DateTime CreatedAt);

public record Recommendation(RekrutmenSummary Rekrutmen, IReadOnlyList<string> SkillCocok);

public record DashboardData(
    int PelamarMenunggu,
    int LamaranMenunggu,
    int RekrutmenTerbuka,
    int TimAktif,
    double? RatingSaya,
    int JumlahUlasan,
    IReadOnlyList<PendingApplicant> PerluKeputusan,
    IReadOnlyList<Recommendation> Rekomendasi,
    IReadOnlyList<RekrutmenSummary> TutupSegera,
    IReadOnlyList<MonthBucket> PerBulan,
    IReadOnlyList<SkillDemand> KeahlianDicari,
    StatusCount PelamarSaya,
    StatusCount LamaranSaya);

public class DashboardService(IDbContextFactory<EtcDbContext> factory, Session session)
{
    static readonly CultureInfo Id = CultureInfo.GetCultureInfo("id-ID");

    public async Task<DashboardData> LoadAsync(DateTime? now = null)
    {
        var today = (now ?? DateTime.Now).Date;
        var me = session.UserId;
        await using var db = await factory.CreateDbContextAsync();

        var mySkills = (await db.Users.Where(u => u.UserId == me).Select(u => u.Spesialisasi).FirstAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var open = await RekrutmenService.Project(
                db.Rekrutmens.Where(r => r.IsOpen && r.BatasPendaftaran >= today), me)
            .ToListAsync();

        var pendingRows = await db.Pendaftars
            .Where(p => p.Rekrutmen.UserId == me && p.Status == StatusPendaftar.Pending)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new { p.PendaftarId, p.RekrutmenId, p.User.Nama, p.User.Jurusan, p.User.Spesialisasi, p.Rekrutmen.Judul, p.Rekrutmen.Skills, p.CreatedAt })
            .ToListAsync();
        var perluKeputusan = pendingRows.Select(p => new PendingApplicant(
            p.PendaftarId, p.RekrutmenId, p.Nama, p.Jurusan, p.Judul,
            p.Spesialisasi.Where(s => p.Skills.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList(),
            p.Skills.Count, p.CreatedAt)).ToList();

        var myAppStatuses = await db.Pendaftars.Where(p => p.UserId == me).Select(p => p.Status).ToListAsync();
        var myRekStatuses = await db.Pendaftars.Where(p => p.Rekrutmen.UserId == me).Select(p => p.Status).ToListAsync();

        var appliedIds = await db.Pendaftars.Where(p => p.UserId == me).Select(p => p.RekrutmenId).ToListAsync();
        var rekomendasi = open
            .Where(r => r.PembuatId != me && !appliedIds.Contains(r.Id))
            .Select(r => new Recommendation(r, r.Skills.Where(mySkills.Contains).ToList()))
            .Where(x => x.SkillCocok.Count > 0)
            .OrderByDescending(x => x.SkillCocok.Count).ThenBy(x => x.Rekrutmen.BatasPendaftaran)
            .Take(4).ToList();

        var tutupSegera = open
            .Where(r => r.PembuatId != me && (r.BatasPendaftaran - today).TotalDays <= 14)
            .OrderBy(r => r.BatasPendaftaran).ThenByDescending(r => r.IsBookmarked)
            .Take(5).ToList();

        var timAktif = await db.Tims.CountAsync(t =>
            t.Participants.Any(p => p.UserId == me) && t.Participants.Count > 1 && t.Rekrutmen.TanggalSelesai >= today);

        var ratings = await db.Histories.Where(h => h.UserId == me).Select(h => h.Rating).ToListAsync();

        var start = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
        var created = await db.Rekrutmens.Where(r => r.CreatedAt >= start)
            .Select(r => new { r.CreatedAt, r.Kegiatan }).ToListAsync();
        var perBulan = Enumerable.Range(0, 12).Select(i =>
        {
            var m = start.AddMonths(i);
            var inMonth = created.Where(c => c.CreatedAt.Year == m.Year && c.CreatedAt.Month == m.Month).ToList();
            return new MonthBucket(
                m.ToString("MMM", Id),
                m.ToString("MMMM yyyy", Id),
                inMonth.Count(c => c.Kegiatan == Kegiatan.Lomba),
                inMonth.Count(c => c.Kegiatan == Kegiatan.Riset),
                inMonth.Count(c => c.Kegiatan == Kegiatan.Projek));
        }).ToList();

        var keahlian = open.SelectMany(r => r.Skills)
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Select(g => new SkillDemand(g.Key, g.Count(), mySkills.Contains(g.Key)))
            .OrderByDescending(s => s.Count).ThenBy(s => s.Skill)
            .Take(8).ToList();

        return new DashboardData(
            perluKeputusan.Count,
            myAppStatuses.Count(s => s == StatusPendaftar.Pending),
            open.Count,
            timAktif,
            ratings.Count > 0 ? ratings.Average() : null,
            ratings.Count,
            perluKeputusan.Take(5).ToList(),
            rekomendasi,
            tutupSegera,
            perBulan,
            keahlian,
            Count(myRekStatuses),
            Count(myAppStatuses));
    }

    static StatusCount Count(List<StatusPendaftar> s) => new(
        s.Count(x => x == StatusPendaftar.Pending),
        s.Count(x => x == StatusPendaftar.Accepted),
        s.Count(x => x == StatusPendaftar.Rejected));
}
