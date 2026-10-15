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

    [ObservableProperty] private string _nama;
    [ObservableProperty] private string? _jurusan;
    [ObservableProperty] private string _noTelp;
    [ObservableProperty] private string? _bio;
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

    public string Initials => Format.Initials(Nama);
    public string Identitas => $"{Labels.Of(_user.Role)} · {_user.NoPengenal}";
    public string Bergabung => $"Bergabung {Format.Date(_user.CreatedAt)}";
    public string RatingText => Rating is { } r ? r.ToString("0.0") : "–";
    public string RatingCaption => Reviews.Count == 0 ? "Belum ada ulasan. Ulasan muncul setelah kegiatan tim selesai." : $"Rata-rata dari {Reviews.Count} ulasan rekan tim";
    public bool HasReviews => Reviews.Count > 0;

    public ProfileViewModel(ShellViewModel shell, AuthService auth, AppSettings settings, Session session, PendaftarService pendaftar)
    {
        _shell = shell;
        _auth = auth;
        _settings = settings;
        _user = session.CurrentUser!;
        _nama = _user.Nama;
        _jurusan = _user.Jurusan;
        _noTelp = _user.NoTelp;
        _bio = _user.Bio;
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
    partial void OnNamaChanged(string value) => OnPropertyChanged(nameof(Initials));

    [RelayCommand]
    async Task Save()
    {
        IsBusy = true;
        Error = null;
        var r = await _auth.UpdateProfileAsync(new ProfileUpdate(Nama, Jurusan, NoTelp, Bio, Skills.Items.ToList()));
        IsBusy = false;
        if (r.Ok) _shell.ShowToast("Profil disimpan.");
        else Error = r.Error;
    }

    [RelayCommand]
    void Logout() => _auth.Logout();
}
