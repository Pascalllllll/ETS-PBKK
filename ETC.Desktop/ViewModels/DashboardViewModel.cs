using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public class PendingRow(PendingApplicant p)
{
    public PendingApplicant Data { get; } = p;
    public string Nama => Data.Nama;
    public string Initials => Format.Initials(Data.Nama);
    public string Judul => Data.Judul;
    public string Meta => $"{Data.Jurusan ?? "Jurusan belum diisi"} · melamar {Format.Ago(Data.CreatedAt)}";
    public string MatchText => Data.SkillDiminta == 0 ? "" : $"{Data.SkillCocok.Count} dari {Data.SkillDiminta} keahlian cocok";
    public bool StrongMatch => Data.SkillDiminta > 0 && Data.SkillCocok.Count * 2 >= Data.SkillDiminta;
}

public class RecommendationRow(Recommendation r)
{
    public RekrutmenRow Row { get; } = new(r.Rekrutmen);
    public IReadOnlyList<SkillChip> Chips { get; } =
        r.Rekrutmen.Skills.Select(s => new SkillChip(s, r.SkillCocok.Contains(s))).ToList();
}

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly DashboardService _service;
    private readonly PendaftarService _pendaftar;
    private readonly Session _session;

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private DashboardData? _data;
    [ObservableProperty] private bool _showTable;

    public ObservableCollection<PendingRow> PerluKeputusan { get; } = new();
    public ObservableCollection<RecommendationRow> Rekomendasi { get; } = new();
    public ObservableCollection<RekrutmenRow> TutupSegera { get; } = new();

    public string Greeting { get; }
    public string Today { get; } = DateTime.Now.ToString("dddd, d MMMM yyyy", Format.Id);

    public DashboardViewModel(ShellViewModel shell, DashboardService service, PendaftarService pendaftar, Session session)
    {
        _shell = shell;
        _service = service;
        _pendaftar = pendaftar;
        _session = session;
        Greeting = $"{Format.Greeting(DateTime.Now)}, {Format.FirstName(session.CurrentUser?.Nama ?? "")}";
        _ = LoadAsync();
    }

    public string HeadlineA => Data is { PelamarMenunggu: > 0 } d ? $"\u2009{d.PelamarMenunggu} pelamar\u2009" : "";
    public string HeadlineARest => Data?.PelamarMenunggu > 0
        ? " menunggu keputusan Anda. "
        : "Tidak ada pelamar yang menunggu keputusan Anda. ";
    public string HeadlineB => Data is null ? "" : Data.Rekomendasi.Count switch
    {
        0 => "Belum ada rekrutmen terbuka yang cocok dengan keahlian di profil Anda.",
        var n => $"{n} rekrutmen terbuka cocok dengan keahlian Anda.",
    };

    public string RatingText => Data?.RatingSaya is { } r ? r.ToString("0.0") : "–";
    public string RatingCaption => Data is null ? "" : Data.JumlahUlasan == 0 ? "Belum ada ulasan" : $"dari {Data.JumlahUlasan} ulasan rekan tim";

    public bool HasPerluKeputusan => PerluKeputusan.Count > 0;
    public bool HasRekomendasi => Rekomendasi.Count > 0;
    public bool HasTutupSegera => TutupSegera.Count > 0;
    public bool HasPelamarSaya => Data?.PelamarSaya.Total > 0;
    public bool HasLamaranSaya => Data?.LamaranSaya.Total > 0;
    public bool HasKeahlian => Data?.KeahlianDicari.Count > 0;
    public string PerluKeputusanMore => Data is { PelamarMenunggu: > 5 } d ? $"Lihat {d.PelamarMenunggu - 5} pelamar lainnya" : "";
    public bool HasPerluKeputusanMore => Data?.PelamarMenunggu > 5;
    public int TotalPerBulan => Data?.PerBulan.Sum(m => m.Total) ?? 0;
    public string PerBulanCaption => Data is null ? "" :
        $"{TotalPerBulan} rekrutmen dibuka dalam 12 bulan terakhir. " +
        $"Lomba {Data.PerBulan.Sum(m => m.Lomba)}, riset {Data.PerBulan.Sum(m => m.Riset)}, projek {Data.PerBulan.Sum(m => m.Projek)}.";

    [RelayCommand]
    async Task LoadAsync()
    {
        IsLoading = true;
        Error = null;
        try
        {
            Data = await _service.LoadAsync();
            PerluKeputusan.Clear();
            foreach (var p in Data.PerluKeputusan) PerluKeputusan.Add(new PendingRow(p));
            Rekomendasi.Clear();
            foreach (var r in Data.Rekomendasi) Rekomendasi.Add(new RecommendationRow(r));
            TutupSegera.Clear();
            foreach (var r in Data.TutupSegera) TutupSegera.Add(new RekrutmenRow(r));
        }
        catch (Exception ex)
        {
            Error = Format.Failure("Ringkasan", ex);
        }
        finally
        {
            IsLoading = false;
            foreach (var n in new[]
                     {
                         nameof(HeadlineA), nameof(HeadlineARest), nameof(HeadlineB),
                         nameof(RatingText), nameof(RatingCaption), nameof(HasPerluKeputusan), nameof(HasRekomendasi),
                         nameof(HasTutupSegera), nameof(HasPelamarSaya), nameof(HasLamaranSaya), nameof(HasKeahlian),
                         nameof(PerluKeputusanMore), nameof(HasPerluKeputusanMore), nameof(PerBulanCaption), nameof(TotalPerBulan),
                     })
                OnPropertyChanged(n);
        }
    }

    [RelayCommand]
    async Task Accept(PendingRow row) => await Decide(row, true);

    [RelayCommand]
    async Task Reject(PendingRow row) => await Decide(row, false);

    async Task Decide(PendingRow row, bool accept)
    {
        var r = await _pendaftar.DecideAsync(row.Data.PendaftarId, accept);
        if (!r.Ok) { _shell.ShowToast(r.Error!); return; }
        _shell.ShowToast(accept ? $"{row.Nama} diterima dan masuk tim." : $"Lamaran {row.Nama} ditolak.");
        await LoadAsync();
        await _shell.RefreshBadgesAsync();
    }

    [RelayCommand]
    void OpenApplicant(PendingRow row) => _shell.NavigateTo(Page.RekrutmenSaya, row.Data.RekrutmenId);

    [RelayCommand]
    void OpenRekrutmen(RekrutmenRow row) => _shell.NavigateTo(Page.Jelajahi, row.Id);

    [RelayCommand]
    void OpenAllApplicants() => _shell.Navigate(Page.RekrutmenSaya);

    [RelayCommand]
    void Explore() => _shell.Navigate(Page.Jelajahi);

    [RelayCommand]
    void EditProfile() => _shell.Navigate(Page.Profil);

    [RelayCommand]
    void OpenApplications() => _shell.Navigate(Page.LamaranSaya);

    [RelayCommand]
    void OpenTeams() => _shell.Navigate(Page.TimSaya);
}
