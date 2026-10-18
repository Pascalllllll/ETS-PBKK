using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public record AppearanceOption(Appearance Value, string Label)
{
    public override string ToString() => Label;
}

public partial class ProfileViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly AuthService _auth;
    private readonly AppSettings _settings;
    private readonly User _user;
    private readonly Session _session;

    [ObservableProperty] private string _nama;
    [ObservableProperty] private string? _jurusan;
    [ObservableProperty] private string _noTelp;
    [ObservableProperty] private string? _bio;
    [ObservableProperty] private decimal? _angkatan;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PengalamanCount))] private string? _pengalaman;
    [ObservableProperty] private string? _linkedInUrl;
    [ObservableProperty] private string? _portofolioUrl;
    [ObservableProperty] private bool _isMentor;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasFoto))] private string? _fotoPath;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private AppearanceOption _appearance;
    [ObservableProperty] private double? _rating;

    public TagListViewModel Skills { get; }
    public ObservableCollection<ReviewItem> Reviews { get; } = new();
    public IReadOnlyList<string> JurusanOptions => Labels.Jurusan;
    public IReadOnlyList<AppearanceOption> AppearanceOptions { get; } =
    [
        new(Services.Appearance.System, "Ikuti sistem"),
        new(Services.Appearance.Light, "Terang"),
        new(Services.Appearance.Dark, "Gelap"),
    ];

    public bool HasFoto => FotoPath is not null;
    public string PengalamanCount => $"{Pengalaman?.Length ?? 0}/1000";
    public int MaxAngkatan => DateTime.Today.Year;
    public string Identitas => $"NRP {_user.NoPengenal}";
    public string Bergabung => $"Bergabung {Format.Date(_user.CreatedAt)}";
    public string RatingText => Rating is { } r ? r.ToString("0.0") : "–";
    public string RatingCaption => Reviews.Count == 0 ? "Belum ada ulasan. Ulasan muncul setelah kegiatan tim selesai." : $"Rata-rata dari {Reviews.Count} ulasan rekan tim";
    public bool HasReviews => Reviews.Count > 0;

    public ProfileViewModel(ShellViewModel shell, AuthService auth, AppSettings settings, Session session, PendaftarService pendaftar)
    {
        _shell = shell;
        _auth = auth;
        _settings = settings;
        _session = session;
        _user = session.CurrentUser!;
        _nama = _user.Nama;
        _jurusan = _user.Jurusan;
        _noTelp = _user.NoTelp;
        _bio = _user.Bio;
        _angkatan = _user.Angkatan;
        _pengalaman = _user.Pengalaman;
        _linkedInUrl = _user.LinkedInUrl;
        _portofolioUrl = _user.PortofolioUrl;
        _isMentor = _user.IsMentor;
        _fotoPath = _user.FotoPath;
        Skills = new TagListViewModel(_user.Spesialisasi);
        _appearance = AppearanceOptions.First(a => a.Value == settings.Appearance);
        _ = LoadReviews(pendaftar);
    }

    async Task LoadReviews(PendaftarService pendaftar)
    {
        foreach (var r in await pendaftar.ReviewsForAsync(_user.UserId)) Reviews.Add(r);
        Rating = Reviews.Count == 0 ? null : Reviews.Average(r => r.Rating);
        OnPropertyChanged(nameof(RatingText));
        OnPropertyChanged(nameof(RatingCaption));
        OnPropertyChanged(nameof(HasReviews));
    }

    partial void OnAppearanceChanged(AppearanceOption value) => _settings.SetAppearance(value.Value);

    [RelayCommand]
    async Task Save()
    {
        IsBusy = true;
        Error = null;
        var r = await _auth.UpdateProfileAsync(new ProfileUpdate(Nama, Jurusan, NoTelp, Bio, Skills.Items.ToList(),
            Angkatan is { } a ? (int)a : null, Pengalaman, LinkedInUrl, PortofolioUrl, IsMentor));
        IsBusy = false;
        if (r.Ok) _shell.ShowToast(IsMentor ? "Profil disimpan. Anda tampil di halaman Mentor." : "Profil disimpan.");
        else Error = r.Error;
    }

    [RelayCommand]
    async Task ChangeFoto()
    {
        var file = await _shell.PickImageAsync("Pilih foto profil");
        if (file is null) return;
        await using var stream = await file.OpenReadAsync();
        var r = await _auth.SetFotoAsync(stream, file.Name);
        if (!r.Ok) { Error = r.Error; return; }
        Error = null;
        FotoPath = _session.CurrentUser?.FotoPath;
        _shell.ShowToast("Foto profil diperbarui.");
    }

    [RelayCommand]
    async Task RemoveFoto()
    {
        await _auth.RemoveFotoAsync();
        FotoPath = null;
    }

    [RelayCommand]
    void Logout() => _auth.Logout();
}
