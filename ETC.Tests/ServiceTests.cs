using ETC.Desktop;
using ETC.Desktop.Data;
using ETC.Desktop.Models;
using ETC.Desktop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ETC.Tests;

public sealed class TestDb : IDisposable
{
    readonly string _path = Path.Combine(Path.GetTempPath(), $"etc-test-{Guid.NewGuid():N}.db");
    public ServiceProvider Sp { get; }

    public TestDb(bool seed = true)
    {
        Sp = App.BuildServices($"Data Source={_path};Pooling=False");
        using var db = Sp.GetRequiredService<IDbContextFactory<EtcDbContext>>().CreateDbContext();
        db.Database.EnsureCreated();
        if (seed) Seeder.Seed(db);
    }

    public T Get<T>() where T : notnull => Sp.GetRequiredService<T>();
    public EtcDbContext Db() => Get<IDbContextFactory<EtcDbContext>>().CreateDbContext();

    public async Task LoginAs(string noPengenal) =>
        Assert.True((await Get<AuthService>().LoginAsync(noPengenal, Seeder.DemoPassword)).Ok);

    public void Dispose()
    {
        Sp.Dispose();
        try { File.Delete(_path); } catch (IOException) { }
    }
}

public class AuthTests
{
    [Fact]
    public async Task Login_dengan_kata_sandi_salah_ditolak()
    {
        using var t = new TestDb();
        var r = await t.Get<AuthService>().LoginAsync("5025241177", "salah123");
        Assert.False(r.Ok);
        Assert.Null(t.Get<Session>().CurrentUser);
    }

    [Fact]
    public async Task Register_lalu_otomatis_masuk()
    {
        using var t = new TestDb(seed: false);
        var r = await t.Get<AuthService>().RegisterAsync(new RegisterRequest(
            "Mahasiswa Baru", UserRole.Mahasiswa, "5025991234", "Teknik Informatika", "081211112222",
            ["Go", "go", "Docker"], "rahasia123", "rahasia123"));
        Assert.True(r.Ok, r.Error);
        var user = t.Get<Session>().CurrentUser!;
        Assert.Equal("Mahasiswa Baru", user.Nama);
        Assert.Equal(2, user.Spesialisasi.Count); // duplikat tanpa memandang huruf besar dibuang
    }

    [Fact]
    public async Task Register_NRP_ganda_ditolak()
    {
        using var t = new TestDb();
        var r = await t.Get<AuthService>().RegisterAsync(new RegisterRequest(
            "Siapa Saja", UserRole.Mahasiswa, "5025241177", null, "081211112222", [], "rahasia123", "rahasia123"));
        Assert.False(r.Ok);
        Assert.Contains("sudah terdaftar", r.Error);
    }

    [Theory]
    [InlineData("0812", false)]
    [InlineData("081234567890", true)]
    [InlineData("6281234567890", true)]
    [InlineData("+6281234567890", false)]
    public void Validasi_nomor_WhatsApp(string no, bool valid) =>
        Assert.Equal(valid, AuthService.ValidateContact(no) is null);

    [Theory]
    [InlineData(UserRole.Mahasiswa, "5025241177", true)]
    [InlineData(UserRole.Mahasiswa, "502524117", false)]
    [InlineData(UserRole.Mahasiswa, "NIM2021001", false)]
    [InlineData(UserRole.Dosen, "19810620 200501 1 003", true)]
    [InlineData(UserRole.Dosen, "198106202005011003", true)]
    [InlineData(UserRole.Dosen, "19810620 200501", false)]
    public void Validasi_format_NRP_dan_NIP(UserRole role, string no, bool valid) =>
        Assert.Equal(valid, AuthService.ValidateIdentity("Nama Lengkap", role, no) is null);

    [Fact]
    public async Task NIP_bisa_diketik_tanpa_spasi()
    {
        using var t = new TestDb();
        Assert.True((await t.Get<AuthService>().LoginAsync("198106202005011003", Seeder.DemoPassword)).Ok);
        Assert.StartsWith("Prof. Ir. Ary", t.Get<Session>().CurrentUser!.Nama);
    }

    [Fact]
    public void Gelar_tidak_masuk_sapaan_dan_inisial()
    {
        const string nama = "Prof. Ir. Ary Mazharuddin Shiddiqi, S.Kom., M.Comp.Sc., Ph.D., IPM.";
        Assert.Equal("Ary", ETC.Desktop.ViewModels.Format.FirstName(nama));
        Assert.Equal("AM", ETC.Desktop.ViewModels.Format.Initials(nama));
    }
}

public class RekrutmenTests
{
    static RekrutmenInput Valid() => new(Kegiatan.Lomba, "Gemastik", "Backend Developer", "Deskripsi", "Go dasar",
        ["Go"], 2, DateTime.Today.AddDays(7), DateTime.Today.AddDays(10), DateTime.Today.AddDays(60), null, "wa.me/6281");

    [Fact]
    public void Validasi_tanggal_berurutan()
    {
        Assert.Null(RekrutmenService.Validate(Valid()));
        Assert.NotNull(RekrutmenService.Validate(Valid() with { TanggalMulai = DateTime.Today.AddDays(3) }));
        Assert.NotNull(RekrutmenService.Validate(Valid() with { TanggalSelesai = DateTime.Today.AddDays(9) }));
        Assert.NotNull(RekrutmenService.Validate(Valid() with { Skills = [] }));
        Assert.NotNull(RekrutmenService.Validate(Valid() with { Kuota = 0 }));
    }

    [Fact]
    public async Task Membuat_rekrutmen_menjadikan_pembuat_ketua_tim()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241133");
        var r = await t.Get<RekrutmenService>().CreateAsync(Valid());
        Assert.True(r.Ok, r.Error);

        using var db = t.Db();
        var tim = db.Tims.Include(x => x.Participants).Single(x => x.RekrutmenId == r.Value);
        var ketua = Assert.Single(tim.Participants);
        Assert.Equal(1, ketua.MemberKe);
        Assert.Equal(t.Get<Session>().UserId, ketua.UserId);
    }

    [Fact]
    public async Task Pencarian_dan_filter_kegiatan()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241177");
        var svc = t.Get<RekrutmenService>();
        var lomba = await svc.SearchAsync(new RekrutmenQuery(Kegiatan: Kegiatan.Lomba, OnlyOpen: false, PageSize: 100));
        Assert.All(lomba.Items, r => Assert.Equal(Kegiatan.Lomba, r.Kegiatan));
        var python = await svc.SearchAsync(new RekrutmenQuery(Text: "python", OnlyOpen: false, PageSize: 100));
        Assert.NotEmpty(python.Items);
        Assert.All(python.Items, r => Assert.Contains(r.Skills.Concat([r.Judul, r.Role]), s => s.Contains("Python", StringComparison.OrdinalIgnoreCase)));
        var open = await svc.SearchAsync(new RekrutmenQuery(PageSize: 100));
        Assert.All(open.Items, r => Assert.True(r.IsOpen && r.BatasPendaftaran >= DateTime.Today));
    }

    [Fact]
    public async Task Bookmark_bisa_dipasang_dan_dilepas()
    {
        using var t = new TestDb();
        await t.LoginAs("5025231101");
        var svc = t.Get<RekrutmenService>();
        var target = (await svc.SearchAsync(new RekrutmenQuery())).Items[0];
        Assert.True(await svc.ToggleBookmarkAsync(target.Id));
        Assert.Contains(await svc.BookmarkedAsync(), r => r.Id == target.Id);
        Assert.False(await svc.ToggleBookmarkAsync(target.Id));
        Assert.DoesNotContain(await svc.BookmarkedAsync(), r => r.Id == target.Id);
    }

    [Fact]
    public async Task Hanya_pemilik_yang_bisa_menghapus()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241145");
        var other = (await t.Get<RekrutmenService>().SearchAsync(new RekrutmenQuery())).Items.First(r => r.PembuatId != t.Get<Session>().UserId);
        Assert.False((await t.Get<RekrutmenService>().DeleteAsync(other.Id)).Ok);
    }
}

public class LamaranTests
{
    [Fact]
    public async Task Alur_lamar_terima_masuk_tim_dan_kuota_menutup()
    {
        using var t = new TestDb(seed: false);
        var auth = t.Get<AuthService>();
        await auth.RegisterAsync(new RegisterRequest("Ketua Tim", UserRole.Dosen, "19900101 201501 1 001", null, "081200000001", ["Go"], "rahasia123", "rahasia123"));
        var rek = await t.Get<RekrutmenService>().CreateAsync(new RekrutmenInput(Kegiatan.Riset, "Riset", "Asisten", "d", "k", ["Go"], 1,
            DateTime.Today.AddDays(5), DateTime.Today.AddDays(6), DateTime.Today.AddDays(30), 100000, "wa.me/6281"));
        auth.Logout();

        var pendaftar = t.Get<PendaftarService>();
        var input = new ApplyInput("Saya tertarik", "https://drive.google.com/cv", "https://github.com/x");
        await auth.RegisterAsync(new RegisterRequest("Mhs Satu", UserRole.Mahasiswa, "5025000001", null, "081200000002", ["Go"], "rahasia123", "rahasia123"));
        Assert.True((await pendaftar.ApplyAsync(rek.Value, input)).Ok);
        Assert.False((await pendaftar.ApplyAsync(rek.Value, input)).Ok); // tidak boleh dua kali
        auth.Logout();
        await auth.RegisterAsync(new RegisterRequest("Mhs Dua", UserRole.Mahasiswa, "5025000002", null, "081200000003", [], "rahasia123", "rahasia123"));
        Assert.True((await pendaftar.ApplyAsync(rek.Value, input)).Ok);
        auth.Logout();

        await auth.LoginAsync("199001012015011001", "rahasia123");
        var applicants = await pendaftar.ApplicantsAsync(rek.Value);
        Assert.Equal(2, applicants.Count);
        var satu = applicants.Single(a => a.Nama == "Mhs Satu");
        Assert.True((await pendaftar.DecideAsync(satu.PendaftarId, true)).Ok);
        var dua = applicants.Single(a => a.Nama == "Mhs Dua");
        var full = await pendaftar.DecideAsync(dua.PendaftarId, true);
        Assert.False(full.Ok);
        Assert.Contains("Kuota", full.Error);

        using var db = t.Db();
        var r = db.Rekrutmens.Include(x => x.Tim!).ThenInclude(x => x.Participants).Single(x => x.RekrutmenId == rek.Value);
        Assert.False(r.IsOpen);
        Assert.Equal(2, r.Tim!.Participants.Count);
        Assert.Contains(r.Tim.Participants, p => p.UserId == satu.UserId && p.MemberKe == 2);
    }

    [Fact]
    public async Task Tidak_bisa_melamar_rekrutmen_sendiri_atau_yang_tertutup()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241177");
        var input = new ApplyInput(null, "https://a.b/cv", "https://a.b/p");
        var mine = (await t.Get<RekrutmenService>().MineAsync()).First();
        Assert.False((await t.Get<PendaftarService>().ApplyAsync(mine.Id, input)).Ok);
        var closed = (await t.Get<RekrutmenService>().SearchAsync(new RekrutmenQuery(OnlyOpen: false, PageSize: 100)))
            .Items.First(r => !r.IsOpen);
        Assert.False((await t.Get<PendaftarService>().ApplyAsync(closed.Id, input)).Ok);
    }

    [Theory]
    [InlineData("drive.google.com/cv", false)]
    [InlineData("https://drive.google.com/cv", true)]
    public void Tautan_CV_harus_URL(string url, bool valid) =>
        Assert.Equal(valid, PendaftarService.Validate(new ApplyInput(null, url, "https://github.com/x")) is null);

    [Fact]
    public async Task Membatalkan_hanya_lamaran_yang_menunggu()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241177");
        var svc = t.Get<PendaftarService>();
        var apps = await svc.MyApplicationsAsync();
        var decided = apps.First(a => a.Status != StatusPendaftar.Pending);
        Assert.False((await svc.WithdrawAsync(decided.PendaftarId)).Ok);
        var pending = apps.First(a => a.Status == StatusPendaftar.Pending);
        Assert.True((await svc.WithdrawAsync(pending.PendaftarId)).Ok);
        Assert.Equal(apps.Count - 1, (await svc.MyApplicationsAsync()).Count);
    }
}

public class TimDanDashboardTests
{
    [Fact]
    public async Task Ulasan_hanya_untuk_rekan_satu_tim_dan_sekali()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241177");
        var tim = t.Get<TimService>();
        var teams = await tim.MyTeamsAsync();
        var target = teams.SelectMany(x => x.Members.Select(m => (x, m))).First(p => !p.m.IsMe && p.m.RatingDariSaya is null);
        Assert.True((await tim.RateAsync(target.x.TimId, target.m.UserId, 4, "Kerja sama yang baik sekali.")).Ok);
        Assert.False((await tim.RateAsync(target.x.TimId, target.m.UserId, 5, "Coba menilai lagi dua kali.")).Ok);
        Assert.False((await tim.RateAsync(target.x.TimId, t.Get<Session>().UserId, 5, "Menilai diri sendiri.")).Ok);
        Assert.NotNull(TimService.ValidateRating(0, "cukup panjang ulasan"));
        Assert.NotNull(TimService.ValidateRating(3, "pendek"));
    }

    [Fact]
    public async Task Dashboard_angka_sesuai_data()
    {
        using var t = new TestDb();
        await t.LoginAs("5025241177");
        var d = await t.Get<DashboardService>().LoadAsync();
        var me = t.Get<Session>().UserId;

        using var db = t.Db();
        Assert.Equal(db.Pendaftars.Count(p => p.Rekrutmen.UserId == me && p.Status == StatusPendaftar.Pending), d.PelamarMenunggu);
        Assert.Equal(db.Pendaftars.Count(p => p.UserId == me && p.Status == StatusPendaftar.Pending), d.LamaranMenunggu);
        Assert.Equal(12, d.PerBulan.Count);
        Assert.Equal(d.PelamarSaya.Total, db.Pendaftars.Count(p => p.Rekrutmen.UserId == me));
        Assert.All(d.Rekomendasi, r =>
        {
            Assert.NotEqual(me, r.Rekrutmen.PembuatId);
            Assert.NotEmpty(r.SkillCocok);
            Assert.False(db.Pendaftars.Any(p => p.UserId == me && p.RekrutmenId == r.Rekrutmen.Id));
        });
        Assert.True(d.KeahlianDicari.SequenceEqual(d.KeahlianDicari.OrderByDescending(s => s.Count).ThenBy(s => s.Skill)));
    }
}
