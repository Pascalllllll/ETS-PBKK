using ETC.Desktop.Data;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace ETC.Desktop.Services;

public enum RekrutmenSort { Terbaru, TenggatTerdekat, KuotaTerbanyak }

public record RekrutmenQuery(
    string? Text = null,
    Kegiatan? Kegiatan = null,
    bool OnlyOpen = true,
    RekrutmenSort Sort = RekrutmenSort.Terbaru,
    int Page = 1,
    int PageSize = 12);

public record RekrutmenInput(
    Kegiatan Kegiatan, string Judul, string Role, string Deskripsi, string Kriteria,
    IReadOnlyList<string> Skills, int Kuota, DateTime BatasPendaftaran,
    DateTime TanggalMulai, DateTime TanggalSelesai, decimal? Fee, string ContactPerson, string? LinkDetail = null);

public record RekrutmenSummary(
    Guid Id, Kegiatan Kegiatan, string Judul, string Role, IReadOnlyList<string> Skills,
    string PembuatNama, UserRole PembuatRole, Guid PembuatId,
    DateTime BatasPendaftaran, DateTime TanggalMulai, DateTime TanggalSelesai,
    decimal? Fee, int Kuota, int Terisi, int JumlahPendaftar, int Menunggu, bool IsOpen, bool IsBookmarked,
    DateTime CreatedAt);

public record RekrutmenDetail(
    RekrutmenSummary Summary, string Deskripsi, string Kriteria, string ContactPerson, string? LinkDetail,
    string? PembuatJurusan, double? PembuatRating, int PembuatUlasan,
    StatusPendaftar? StatusSaya, bool IsMine);

public class RekrutmenService(IDbContextFactory<EtcDbContext> factory, Session session)
{
    internal static IQueryable<RekrutmenSummary> Project(IQueryable<Rekrutmen> q, Guid viewer) =>
        q.Select(r => new RekrutmenSummary(
            r.RekrutmenId, r.Kegiatan, r.Judul, r.Role, r.Skills,
            r.User.Nama, r.User.Role, r.UserId,
            r.BatasPendaftaran, r.TanggalMulai, r.TanggalSelesai,
            r.Fee, r.Kuota,
            r.Pendaftar.Count(p => p.Status == StatusPendaftar.Accepted),
            r.Pendaftar.Count,
            r.Pendaftar.Count(p => p.Status == StatusPendaftar.Pending),
            r.IsOpen,
            r.Bookmarks.Any(b => b.UserId == viewer),
            r.CreatedAt));

    public async Task<PagedResult<RekrutmenSummary>> SearchAsync(RekrutmenQuery query)
    {
        await using var db = await factory.CreateDbContextAsync();
        var today = DateTime.Today;
        IQueryable<Rekrutmen> q = db.Rekrutmens;

        if (query.Kegiatan is { } k) q = q.Where(r => r.Kegiatan == k);
        if (query.OnlyOpen) q = q.Where(r => r.IsOpen && r.BatasPendaftaran >= today);

        // Teks dicari di memori karena Skills disimpan sebagai JSON.
        var all = await Project(q, session.UserId).ToListAsync();
        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var t = query.Text.Trim();
            all = all.Where(r =>
                r.Judul.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                r.Role.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                r.PembuatNama.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                r.Skills.Any(s => s.Contains(t, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        all = query.Sort switch
        {
            RekrutmenSort.TenggatTerdekat => all.OrderBy(r => r.BatasPendaftaran).ToList(),
            RekrutmenSort.KuotaTerbanyak => all.OrderByDescending(r => r.Kuota - r.Terisi).ThenBy(r => r.BatasPendaftaran).ToList(),
            _ => all.OrderByDescending(r => r.CreatedAt).ToList(),
        };

        var page = Math.Max(1, query.Page);
        var items = all.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToList();
        return new PagedResult<RekrutmenSummary>(items, page, query.PageSize, all.Count);
    }

    public async Task<RekrutmenDetail?> GetDetailAsync(Guid id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        var summary = await Project(db.Rekrutmens.Where(r => r.RekrutmenId == id), me).FirstOrDefaultAsync();
        if (summary is null) return null;

        var r = await db.Rekrutmens.Include(x => x.User).FirstAsync(x => x.RekrutmenId == id);
        var ratings = await db.Histories.Where(h => h.UserId == r.UserId).Select(h => h.Rating).ToListAsync();
        var mine = await db.Pendaftars.Where(p => p.RekrutmenId == id && p.UserId == me)
            .Select(p => (StatusPendaftar?)p.Status).FirstOrDefaultAsync();

        return new RekrutmenDetail(summary, r.Deskripsi, r.Kriteria, r.ContactPerson, r.LinkDetail, r.User.Jurusan,
            ratings.Count > 0 ? ratings.Average() : null, ratings.Count, mine, r.UserId == me);
    }

    public async Task<Rekrutmen?> GetForEditAsync(Guid id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Rekrutmens.AsNoTracking().FirstOrDefaultAsync(r => r.RekrutmenId == id && r.UserId == session.UserId);
    }

    public static string? Validate(RekrutmenInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Judul)) return "Isi nama kegiatan.";
        if (string.IsNullOrWhiteSpace(i.Role)) return "Isi posisi yang dicari.";
        if (string.IsNullOrWhiteSpace(i.Kriteria)) return "Tulis kriteria pendaftar.";
        if (i.Skills.Count == 0) return "Tambahkan minimal satu keahlian.";
        if (i.Kuota is < 1 or > 20) return "Kuota antara 1 dan 20 orang.";
        if (i.BatasPendaftaran.Date < DateTime.Today) return "Batas pendaftaran tidak boleh di masa lalu.";
        if (i.TanggalMulai.Date < i.BatasPendaftaran.Date) return "Kegiatan dimulai setelah pendaftaran ditutup.";
        if (i.TanggalSelesai.Date < i.TanggalMulai.Date) return "Tanggal selesai harus setelah tanggal mulai.";
        if (i.Fee is < 0) return "Fee tidak boleh negatif.";
        if (string.IsNullOrWhiteSpace(i.ContactPerson)) return "Isi kontak yang bisa dihubungi.";
        if (!string.IsNullOrWhiteSpace(i.LinkDetail) && !PendaftarService.IsUrl(i.LinkDetail))
            return "Link detail harus diawali https://";
        return null;
    }

    public async Task<Result<Guid>> CreateAsync(RekrutmenInput i)
    {
        if (Validate(i) is { } err) return Result<Guid>.Fail(err);
        await using var db = await factory.CreateDbContextAsync();
        var r = new Rekrutmen { UserId = session.UserId };
        Apply(r, i);
        // Pembuat otomatis menjadi ketua tim, seperti di etc-be.
        var tim = new Tim { RekrutmenId = r.RekrutmenId, TipeTim = i.Kegiatan };
        tim.Participants.Add(new TimParticipant { TimId = tim.TimId, UserId = session.UserId, MemberKe = 1 });
        r.Tim = tim;
        db.Rekrutmens.Add(r);
        await db.SaveChangesAsync();
        return Result<Guid>.Success(r.RekrutmenId);
    }

    public async Task<Result> UpdateAsync(Guid id, RekrutmenInput i)
    {
        if (Validate(i) is { } err) return Result.Fail(err);
        await using var db = await factory.CreateDbContextAsync();
        var r = await db.Rekrutmens.Include(x => x.Tim).FirstOrDefaultAsync(x => x.RekrutmenId == id);
        if (r is null || r.UserId != session.UserId) return Result.Fail("Rekrutmen ini bukan milik Anda.");
        var accepted = await db.Pendaftars.CountAsync(p => p.RekrutmenId == id && p.Status == StatusPendaftar.Accepted);
        if (i.Kuota < accepted) return Result.Fail($"Sudah ada {accepted} anggota diterima. Kuota tidak bisa di bawah itu.");
        Apply(r, i);
        if (r.Tim is not null) r.Tim.TipeTim = i.Kegiatan;
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> SetOpenAsync(Guid id, bool open)
    {
        await using var db = await factory.CreateDbContextAsync();
        var r = await db.Rekrutmens.FirstOrDefaultAsync(x => x.RekrutmenId == id);
        if (r is null || r.UserId != session.UserId) return Result.Fail("Rekrutmen ini bukan milik Anda.");
        if (open && r.BatasPendaftaran.Date < DateTime.Today)
            return Result.Fail("Batas pendaftaran sudah lewat. Ubah tanggalnya dulu untuk membuka kembali.");
        r.IsOpen = open;
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var r = await db.Rekrutmens.FirstOrDefaultAsync(x => x.RekrutmenId == id);
        if (r is null || r.UserId != session.UserId) return Result.Fail("Rekrutmen ini bukan milik Anda.");
        db.Rekrutmens.Remove(r);
        await db.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<List<RekrutmenSummary>> MineAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await Project(db.Rekrutmens.Where(r => r.UserId == session.UserId)
            .OrderByDescending(r => r.CreatedAt), session.UserId).ToListAsync();
    }

    public async Task<List<RekrutmenSummary>> BookmarkedAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        return await Project(db.Rekrutmens.Where(r => r.Bookmarks.Any(b => b.UserId == me))
            .OrderBy(r => r.BatasPendaftaran), me).ToListAsync();
    }

    public async Task<bool> ToggleBookmarkAsync(Guid rekrutmenId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var me = session.UserId;
        var existing = await db.Bookmarks.FirstOrDefaultAsync(b => b.RekrutmenId == rekrutmenId && b.UserId == me);
        if (existing is not null) db.Bookmarks.Remove(existing);
        else db.Bookmarks.Add(new Bookmark { RekrutmenId = rekrutmenId, UserId = me });
        await db.SaveChangesAsync();
        return existing is null;
    }

    static void Apply(Rekrutmen r, RekrutmenInput i)
    {
        r.Kegiatan = i.Kegiatan;
        r.Judul = i.Judul.Trim();
        r.Role = i.Role.Trim();
        r.Deskripsi = i.Deskripsi.Trim();
        r.Kriteria = i.Kriteria.Trim();
        r.Skills = i.Skills.Select(s => s.Trim()).Where(s => s.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        r.Kuota = i.Kuota;
        r.BatasPendaftaran = i.BatasPendaftaran.Date;
        r.TanggalMulai = i.TanggalMulai.Date;
        r.TanggalSelesai = i.TanggalSelesai.Date;
        r.Fee = i.Fee;
        r.ContactPerson = i.ContactPerson.Trim();
        r.LinkDetail = string.IsNullOrWhiteSpace(i.LinkDetail) ? null : i.LinkDetail.Trim();
    }
}
