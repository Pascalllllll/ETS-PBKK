using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public partial class ApplySheetViewModel(ShellViewModel shell, PendaftarService pendaftar, Session session, RekrutmenSummary rekrutmen, Action onDone)
    : SheetViewModel
{
    public string Title => $"Lamar sebagai {rekrutmen.Role}";
    public string Subtitle => $"{rekrutmen.Judul} · {rekrutmen.PembuatNama}";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(AlasanCount))] private string _alasan = "";
    [ObservableProperty] private string _cvUrl = "";
    [ObservableProperty] private string _portofolioUrl = session.CurrentUser?.PortofolioUrl ?? "";
    public string AlasanCount => $"{Alasan.Length}/500";

    [RelayCommand]
    async Task Submit()
    {
        var input = new ApplyInput(Alasan, CvUrl, PortofolioUrl);
        Error = PendaftarService.Validate(input);
        if (Error is not null) return;
        IsBusy = true;
        var r = await pendaftar.ApplyAsync(rekrutmen.Id, input);
        IsBusy = false;
        if (!r.Ok) { Error = r.Error; return; }
        Close?.Invoke();
        shell.ShowToast("Lamaran terkirim. Status bisa dipantau di Lamaran saya.");
        onDone();
    }
}

public partial class RatingSheetViewModel(TimService tim, ShellViewModel shell, MemberRow member, Func<Task> onDone) : SheetViewModel
{
    public string Title => $"Ulas {member.Data.Nama}";
    public string Subtitle => $"{member.Tim.Judul} · {member.Peran}";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(RatingLabel))] private int _rating;
    [ObservableProperty] private string _deskripsi = "";

    public string RatingLabel => Rating switch
    {
        0 => "Pilih jumlah bintang",
        1 => "1 dari 5 · Kontribusi sangat kurang",
        2 => "2 dari 5 · Kontribusi kurang",
        3 => "3 dari 5 · Cukup",
        4 => "4 dari 5 · Baik",
        _ => "5 dari 5 · Sangat baik",
    };

    public bool Star1 => Rating >= 1;
    public bool Star2 => Rating >= 2;
    public bool Star3 => Rating >= 3;
    public bool Star4 => Rating >= 4;
    public bool Star5 => Rating >= 5;

    partial void OnRatingChanged(int value)
    {
        foreach (var n in new[] { nameof(Star1), nameof(Star2), nameof(Star3), nameof(Star4), nameof(Star5) }) OnPropertyChanged(n);
    }

    [RelayCommand]
    void SetRating(string value) => Rating = int.Parse(value);

    [RelayCommand]
    async Task Submit()
    {
        Error = TimService.ValidateRating(Rating, Deskripsi);
        if (Error is not null) return;
        IsBusy = true;
        var r = await tim.RateAsync(member.Tim.TimId, member.Data.UserId, Rating, Deskripsi);
        IsBusy = false;
        if (!r.Ok) { Error = r.Error; return; }
        Close?.Invoke();
        shell.ShowToast($"Ulasan untuk {member.Data.Nama} tersimpan.");
        await onDone();
    }
}

public partial class RekrutmenFormViewModel : SheetViewModel
{
    private readonly ShellViewModel _shell;
    private readonly RekrutmenService _service;
    private readonly Guid _id;

    public bool IsEdit => _id != Guid.Empty;
    public string Title => IsEdit ? "Ubah rekrutmen" : "Buka rekrutmen baru";
    public string SubmitLabel => IsEdit ? "Simpan perubahan" : "Buka rekrutmen";

    [ObservableProperty] private Kegiatan _kegiatan = Kegiatan.Lomba;
    [ObservableProperty] private string _judul = "";
    [ObservableProperty] private string _role = "";
    [ObservableProperty] private string _deskripsi = "";
    [ObservableProperty] private string _kriteria = "";
    [ObservableProperty] private decimal? _kuota = 2;
    [ObservableProperty] private DateTime? _batas = DateTime.Today.AddDays(14);
    [ObservableProperty] private DateTime? _mulai = DateTime.Today.AddDays(21);
    [ObservableProperty] private DateTime? _selesai = DateTime.Today.AddDays(111);
    [ObservableProperty] private string _fee = "";
    [ObservableProperty] private string _contact = "";
    [ObservableProperty] private string _linkDetail = "";
    public TagListViewModel Skills { get; } = new();

    public bool IsLomba { get => Kegiatan == Kegiatan.Lomba; set { if (value) Kegiatan = Kegiatan.Lomba; } }
    public bool IsRiset { get => Kegiatan == Kegiatan.Riset; set { if (value) Kegiatan = Kegiatan.Riset; } }
    public bool IsProjek { get => Kegiatan == Kegiatan.Projek; set { if (value) Kegiatan = Kegiatan.Projek; } }

    partial void OnKegiatanChanged(Kegiatan value)
    {
        OnPropertyChanged(nameof(IsLomba)); OnPropertyChanged(nameof(IsRiset)); OnPropertyChanged(nameof(IsProjek));
    }

    public RekrutmenFormViewModel(ShellViewModel shell, RekrutmenService service, Session session, Guid id)
    {
        _shell = shell;
        _service = service;
        _id = id;
        var u = session.CurrentUser!;
        _contact = Format.WhatsAppUrl(u.NoTelp).Replace("https://", "");
        if (IsEdit) _ = LoadAsync();
    }

    async Task LoadAsync()
    {
        var r = await _service.GetForEditAsync(_id);
        if (r is null) { Error = "Rekrutmen tidak ditemukan."; return; }
        Kegiatan = r.Kegiatan;
        Judul = r.Judul;
        Role = r.Role;
        Deskripsi = r.Deskripsi;
        Kriteria = r.Kriteria;
        Kuota = r.Kuota;
        Batas = r.BatasPendaftaran;
        Mulai = r.TanggalMulai;
        Selesai = r.TanggalSelesai;
        Fee = r.Fee is { } f and > 0 ? f.ToString("0") : "";
        Contact = r.ContactPerson;
        LinkDetail = r.LinkDetail ?? "";
        foreach (var s in r.Skills) Skills.Items.Add(s);
    }

    [RelayCommand]
    async Task Submit()
    {
        decimal? fee = null;
        var feeText = Fee.Replace(".", "").Replace("Rp", "").Trim();
        if (feeText.Length > 0)
        {
            if (!decimal.TryParse(feeText, out var f)) { Error = "Fee diisi angka saja, contoh 500000."; return; }
            fee = f;
        }
        if (Batas is null || Mulai is null || Selesai is null) { Error = "Lengkapi semua tanggal."; return; }

        var input = new RekrutmenInput(Kegiatan, Judul, Role, Deskripsi, Kriteria, Skills.Items.ToList(),
            (int)(Kuota ?? 0), Batas.Value, Mulai.Value, Selesai.Value, fee, Contact, LinkDetail);
        Error = RekrutmenService.Validate(input);
        if (Error is not null) return;

        IsBusy = true;
        Guid id = _id;
        if (IsEdit)
        {
            var r = await _service.UpdateAsync(_id, input);
            if (!r.Ok) Error = r.Error;
        }
        else
        {
            var r = await _service.CreateAsync(input);
            if (!r.Ok) Error = r.Error; else id = r.Value;
        }
        IsBusy = false;
        if (Error is not null) return;

        Close?.Invoke();
        _shell.ShowToast(IsEdit ? "Perubahan disimpan." : "Rekrutmen dibuka. Pelamar bisa mendaftar sekarang.");
        _shell.NavigateTo(Page.RekrutmenSaya, id);
    }
}
