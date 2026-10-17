using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ETC.Desktop;
using ETC.Desktop.Services;
using ETC.Desktop.ViewModels;
using ETC.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

[assembly: AvaloniaTestApplication(typeof(ETC.Tests.TestAppBuilder))]

namespace ETC.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        Environment.SetEnvironmentVariable("ETC_DB", Path.Combine(Path.GetTempPath(), $"etc-ui-{Guid.NewGuid():N}.db"));
        return AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
    }
}

public class UiTests
{
    static Window Open()
    {
        App.Services.GetRequiredService<AuthService>().Logout();
        var w = new MainWindow { DataContext = App.Services.GetRequiredService<MainViewModel>(), Width = 1320, Height = 860 };
        w.Show();
        Dispatcher.UIThread.RunJobs();
        return w;
    }

    static async Task Until(Func<bool> cond, string what, int ms = 5000)
    {
        var end = DateTime.Now.AddMilliseconds(ms);
        while (DateTime.Now < end)
        {
            Dispatcher.UIThread.RunJobs();
            if (cond()) return;
            await Task.Delay(25);
        }
        Assert.Fail("Menunggu terlalu lama: " + what);
    }

    static string TextOf(Control c) => c switch
    {
        ContentControl { Content: string s } => s,
        _ => string.Join(" ", c.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text)),
    };

    static T Find<T>(Window w, string text) where T : Control =>
        w.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.IsEffectivelyVisible && TextOf(c).Trim() == text)
        ?? throw new Xunit.Sdk.XunitException($"Kontrol \"{text}\" tidak ditemukan");

    static bool Has(Window w, string text) =>
        w.GetVisualDescendants().OfType<TextBlock>().Any(t => t.IsEffectivelyVisible && t.Text?.Contains(text) == true);

    static void Click(Window w, Control c)
    {
        c.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var p = c.TranslatePoint(new Point(c.Bounds.Width / 2, c.Bounds.Height / 2), w)!.Value;
        w.MouseMove(p);
        w.MouseDown(p, MouseButton.Left);
        w.MouseUp(p, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    static ShellViewModel Shell => (ShellViewModel)App.Services.GetRequiredService<MainViewModel>().Current;

    static async Task<Window> LoginDemo(string which = "Isi akun mahasiswa")
    {
        var w = Open();
        Click(w, Find<Button>(w, which));
        Click(w, Find<Button>(w, "Masuk"));
        await Until(() => App.Services.GetRequiredService<MainViewModel>().Current is ShellViewModel, "masuk ke shell");
        await Until(() => Has(w, "Perlu keputusan Anda"), "ringkasan tampil");
        return w;
    }

    [AvaloniaFact]
    public async Task Login_salah_menampilkan_pesan_lalu_akun_demo_berhasil()
    {
        var w = Open();
        Click(w, Find<Button>(w, "Masuk"));
        await Until(() => Has(w, "Isi NRP/NIP dan kata sandi."), "pesan validasi kosong");
        w.Close();
        await LoginDemo();
    }

    [AvaloniaFact]
    public async Task Onboarding_tiga_langkah_memvalidasi_tiap_langkah()
    {
        var w = Open();
        Click(w, Find<Button>(w, "Buat akun"));
        await Until(() => Has(w, "Siapa Anda?"), "langkah 1");
        Click(w, Find<Button>(w, "Lanjut"));
        await Until(() => Has(w, "Nama minimal 3 karakter."), "validasi langkah 1");
        Click(w, Find<Button>(w, "Kembali"));
        await Until(() => Has(w, "Masuk ke ETC"), "kembali ke layar masuk");
    }

    [AvaloniaFact]
    public async Task Setiap_item_sidebar_membuka_halamannya()
    {
        var w = await LoginDemo();
        var expected = new (string label, Type vm, string marker)[]
        {
            ("Jelajahi", typeof(ExploreViewModel), "Jelajahi rekrutmen"),
            ("Tersimpan", typeof(ExploreViewModel), "Rekrutmen yang Anda simpan"),
            ("Rekrutmen saya", typeof(MyRekrutmenViewModel), "Pilih rekrutmen, tinjau"),
            ("Lamaran saya", typeof(MyApplicationsViewModel), "menunggu ·"),
            ("Tim saya", typeof(TeamsViewModel), "Tim terbentuk dari pelamar"),
            ("Mentor", typeof(MentorViewModel), "Pengalaman dan prestasi"),
            ("Profil dan ulasan", typeof(ProfileViewModel), "Data diri"),
            ("Ringkasan", typeof(DashboardViewModel), "Perlu keputusan Anda"),
        };
        foreach (var (label, vm, marker) in expected)
        {
            var row = w.GetVisualDescendants().OfType<RadioButton>().First(r => r.Classes.Contains("nav") && TextOf(r).StartsWith(label));
            Click(w, row);
            await Until(() => Shell.Content?.GetType() == vm && Has(w, marker), $"halaman {label}");
        }
    }

    [AvaloniaFact]
    public async Task Ctrl_N_membuka_formulir_dan_Escape_menutupnya()
    {
        var w = await LoginDemo();
        w.KeyPress(Key.N, RawInputModifiers.Control, PhysicalKey.N, "n");
        await Until(() => Shell.Sheet is RekrutmenFormViewModel && Has(w, "Buka rekrutmen baru"), "sheet terbuka");
        // Tombol toolbar berlabel sama ada di belakang scrim.
        var form = w.GetVisualDescendants().OfType<RekrutmenFormView>().Single();
        Click(w, form.GetVisualDescendants().OfType<Button>().First(b => TextOf(b) == "Buka rekrutmen"));
        await Until(() => Has(w, "Isi nama kegiatan."), "validasi formulir");
        w.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
        await Until(() => Shell.Sheet is null, "sheet tertutup");
    }

    [AvaloniaFact]
    public async Task Melamar_dari_Jelajahi_sampai_muncul_di_Lamaran_saya()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.Jelajahi);
        var explore = (ExploreViewModel)Shell.Content!;
        await Until(() => explore.Items.Count > 0, "daftar rekrutmen");
        RekrutmenRow? target = null;
        foreach (var row in explore.Items.ToList())
        {
            explore.Selected = row;
            await Until(() => explore.Detail is { IsLoading: false }, "detail dimuat");
            if (explore.Detail!.CanApply) { target = row; break; }
        }
        Assert.NotNull(target);
        Click(w, Find<Button>(w, "Lamar posisi ini"));
        await Until(() => Shell.Sheet is ApplySheetViewModel, "sheet lamaran");

        Click(w, Find<Button>(w, "Kirim lamaran"));
        await Until(() => Has(w, "Tautan CV harus diawali https://"), "validasi tautan");

        var sheet = (ApplySheetViewModel)Shell.Sheet!;
        sheet.CvUrl = "https://drive.google.com/cv/aldi";
        sheet.PortofolioUrl = "https://github.com/aldi";
        Click(w, Find<Button>(w, "Kirim lamaran"));
        await Until(() => Shell.Sheet is null && Shell.Toast?.StartsWith("Lamaran terkirim") == true, "lamaran terkirim");
        await Until(() => Has(w, "Lamaran Anda sudah terkirim"), "status di detail");

        Shell.Navigate(Page.LamaranSaya);
        await Until(() => Has(w, target!.Role), "lamaran muncul di Lamaran saya");
    }

    [AvaloniaFact]
    public async Task Menerima_pelamar_dari_Ringkasan_mengurangi_badge()
    {
        var w = await LoginDemo();
        await Until(() => Shell.PelamarMenunggu > 0, "badge pelamar");
        var before = Shell.PelamarMenunggu;
        Click(w, Find<Button>(w, "Terima"));
        await Until(() => Shell.Toast?.Contains("diterima dan masuk tim") == true, "toast diterima");
        await Until(() => Shell.PelamarMenunggu == before - 1, "badge berkurang");
    }

    [AvaloniaFact]
    public async Task Simpan_rekrutmen_lalu_muncul_di_Tersimpan()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.Jelajahi);
        var explore = (ExploreViewModel)Shell.Content!;
        await Until(() => explore.Detail is { IsLoading: false }, "detail pertama");
        var row = explore.Items.First(i => !i.IsBookmarked);
        explore.Selected = row;
        await Until(() => explore.Detail is { IsLoading: false } d && d.Row?.Id == row.Id, "detail terpilih");
        Click(w, Find<Button>(w, "Simpan"));
        await Until(() => Shell.Toast == "Disimpan ke Tersimpan.", "toast simpan");
        Shell.Navigate(Page.Tersimpan);
        var saved = (ExploreViewModel)Shell.Content!;
        await Until(() => saved.Items.Any(i => i.Id == row.Id), "muncul di Tersimpan");
    }

    [AvaloniaFact]
    public async Task Filter_kegiatan_dan_hapus_filter_di_Jelajahi()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.Jelajahi);
        var explore = (ExploreViewModel)Shell.Content!;
        await Until(() => explore.Items.Count > 0, "daftar");
        Click(w, Find<RadioButton>(w, "Riset"));
        await Until(() => explore.Items.Count > 0 && explore.Items.All(i => i.KegiatanLabel == "Riset"), "filter riset");
        explore.SearchText = "zzz-tidak-ada";
        await Until(() => explore.IsEmpty && Has(w, "Tidak ada rekrutmen yang cocok"), "keadaan kosong");
        Click(w, Find<Button>(w, "Hapus semua filter"));
        await Until(() => explore.Items.Count > 0, "filter dihapus");
    }

    [AvaloniaFact]
    public async Task Mode_gelap_dari_Profil_mengubah_tema()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.Profil);
        var profile = (ProfileViewModel)Shell.Content!;
        profile.Appearance = profile.AppearanceOptions.First(a => a.Value == Appearance.Dark);
        await Until(() => Application.Current!.ActualThemeVariant == ThemeVariant.Dark, "tema gelap");
        profile.Appearance = profile.AppearanceOptions.First(a => a.Value == Appearance.System);
    }

    [AvaloniaFact]
    public async Task Hapus_rekrutmen_meminta_konfirmasi_dan_Batal_tidak_menghapus()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.RekrutmenSaya);
        var mine = (MyRekrutmenViewModel)Shell.Content!;
        await Until(() => mine.Items.Count > 0 && mine.Selected is not null, "rekrutmen saya");
        var count = mine.Items.Count;
        Click(w, w.GetVisualDescendants().OfType<Button>().First(b => b.IsEffectivelyVisible && ToolTip.GetTip(b) as string == "Hapus rekrutmen"));
        await Until(() => Shell.Sheet is ConfirmSheetViewModel, "konfirmasi hapus");
        Click(w, Find<Button>(w, "Batal"));
        await Until(() => Shell.Sheet is null, "batal");
        Assert.Equal(count, mine.Items.Count);
    }

    [AvaloniaFact]
    public async Task Membuat_rekrutmen_lengkap_lalu_tampil_di_Rekrutmen_saya()
    {
        var w = await LoginDemo();
        Shell.NewRekrutmenCommand.Execute(null);
        await Until(() => Shell.Sheet is RekrutmenFormViewModel, "form terbuka");
        var f = (RekrutmenFormViewModel)Shell.Sheet!;
        Click(w, Find<RadioButton>(w, "Projek"));
        f.Judul = "Aplikasi Antrian Klinik Kampus";
        f.Role = "Backend Developer";
        f.Deskripsi = "Membangun API antrian.";
        f.Kriteria = "Paham REST API";
        f.Skills.AddCommand.Execute("Go");
        f.Fee = "500.000";
        var form = w.GetVisualDescendants().OfType<RekrutmenFormView>().Single();
        Click(w, form.GetVisualDescendants().OfType<Button>().First(b => TextOf(b) == "Buka rekrutmen"));
        await Until(() => Shell.Sheet is null && Shell.Content is MyRekrutmenViewModel, "pindah ke Rekrutmen saya");
        var mine = (MyRekrutmenViewModel)Shell.Content!;
        await Until(() => mine.Selected?.Role == "Backend Developer" && mine.Selected.KegiatanLabel == "Projek", "rekrutmen baru terpilih");
        Assert.Equal("Rp500.000", mine.Selected!.FeeText);
        Assert.True(mine.NoApplicants);
    }

    [AvaloniaFact]
    public async Task Memberi_ulasan_rekan_tim_yang_kegiatannya_selesai()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.TimSaya);
        var teams = (TeamsViewModel)Shell.Content!;
        await Until(() => teams.Items.Count > 0, "daftar tim");
        var team = teams.Items.FirstOrDefault(t => t.Members.Any(m => m.CanRate));
        Assert.NotNull(team);
        teams.Selected = team;
        await Until(() => Has(w, "Beri ulasan"), "tombol ulasan");
        Click(w, Find<Button>(w, "Beri ulasan"));
        await Until(() => Shell.Sheet is RatingSheetViewModel, "sheet ulasan");
        Click(w, w.GetVisualDescendants().OfType<Button>().First(b => b.IsEffectivelyVisible && Avalonia.Automation.AutomationProperties.GetName(b) == "4 bintang"));
        await Until(() => ((RatingSheetViewModel)Shell.Sheet!).Rating == 4 && Has(w, "4 dari 5"), "bintang terpilih");
        ((RatingSheetViewModel)Shell.Sheet!).Deskripsi = "Selalu menyelesaikan bagiannya tepat waktu.";
        Click(w, Find<Button>(w, "Kirim ulasan"));
        await Until(() => Shell.Sheet is null && Shell.Toast?.StartsWith("Ulasan untuk") == true, "ulasan tersimpan");
        await Until(() => Has(w, "Anda memberi 4 dari 5"), "status ulasan di daftar anggota");
    }

    [AvaloniaFact]
    public async Task Membatalkan_lamaran_lewat_konfirmasi()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.LamaranSaya);
        var apps = (MyApplicationsViewModel)Shell.Content!;
        await Until(() => apps.Items.Any(a => a.IsPending), "lamaran menunggu");
        var count = apps.Items.Count;
        Click(w, Find<Button>(w, "Batalkan"));
        await Until(() => Shell.Sheet is ConfirmSheetViewModel && Has(w, "Batalkan lamaran?"), "konfirmasi");
        Click(w, Find<Button>(w, "Batalkan lamaran"));
        await Until(() => Shell.Sheet is null && apps.Items.Count == count - 1, "lamaran terhapus");
    }

    [AvaloniaFact]
    public async Task Simpan_profil_memvalidasi_nomor_lalu_keluar()
    {
        var w = await LoginDemo("Isi akun dosen");
        Shell.Navigate(Page.Profil);
        var profile = (ProfileViewModel)Shell.Content!;
        await Until(() => Has(w, "Data diri"), "halaman profil");
        profile.Bio = "Membimbing riset machine learning.";
        profile.NoTelp = "08";
        Click(w, Find<Button>(w, "Simpan profil"));
        await Until(() => Has(w, "Nomor WhatsApp diawali 08 atau 62"), "validasi nomor");
        profile.NoTelp = "081234560001";
        Click(w, Find<Button>(w, "Simpan profil"));
        await Until(() => Shell.Toast?.StartsWith("Profil disimpan.") == true, "profil tersimpan");
        Click(w, Find<Button>(w, "Keluar"));
        await Until(() => App.Services.GetRequiredService<MainViewModel>().Current is AuthViewModel && Has(w, "Masuk ke ETC"), "kembali ke layar masuk");
    }

    [AvaloniaFact]
    public async Task Ringkasan_menampilkan_langkah_onboarding_profil()
    {
        var w = await LoginDemo();
        await Until(() => Has(w, "Lengkapi profil Anda") && Has(w, "Tulis pengalaman atau prestasi"), "kartu onboarding");
        Click(w, Find<Button>(w, "Lengkapi profil"));
        await Until(() => Shell.Content is ProfileViewModel && Has(w, "Pengalaman dan tautan"), "halaman profil");
    }

    [AvaloniaFact]
    public async Task Butuh_mentor_membuka_form_dan_mengirim_permintaan()
    {
        var w = await LoginDemo();
        Shell.Navigate(Page.Mentor);
        await Until(() => Has(w, "Pengalaman dan prestasi"), "daftar mentor");
        Click(w, Find<Button>(w, "Butuh mentor?"));
        await Until(() => Shell.Sheet is MentorRequestSheetViewModel s && s.Mentors.Count > 0, "form mentor");
        var sheet = (MentorRequestSheetViewModel)Shell.Sheet!;

        Click(w, Find<Button>(w, "Kirim permintaan"));
        await Until(() => Has(w, "Pilih mentor yang ingin dihubungi."), "validasi mentor");

        sheet.Mentor = sheet.Mentors.First(m => m.Nama.StartsWith("Cathleen"));
        sheet.Topik = "Analisis data Satria Data";
        sheet.Pesan = "Tim kami butuh arahan memilih metode regresi untuk babak final.";
        Click(w, Find<Button>(w, "Kirim permintaan"));
        await Until(() => Shell.Sheet is null && ((MentorViewModel)Shell.Content!).IsTerkirim, "permintaan terkirim");
        await Until(() => Has(w, "Analisis data Satria Data"), "permintaan tampil di daftar");
    }
}
