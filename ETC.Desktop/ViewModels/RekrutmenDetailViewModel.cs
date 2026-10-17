using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public partial class RekrutmenDetailViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly RekrutmenService _rekrutmen;
    private readonly Action<bool>? _bookmarkChanged;

    [ObservableProperty] private RekrutmenDetail? _detail;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private bool _isBookmarked;

    public ObservableCollection<SkillChip> Skills { get; } = new();

    public RekrutmenDetailViewModel(ShellViewModel shell, RekrutmenService rekrutmen, Session session, Guid id, Action<bool>? bookmarkChanged)
    {
        _shell = shell;
        _rekrutmen = rekrutmen;
        _bookmarkChanged = bookmarkChanged;
        _ = Load(id, session.CurrentUser?.Spesialisasi ?? []);
    }

    async Task Load(Guid id, List<string> mySkills)
    {
        Detail = await _rekrutmen.GetDetailAsync(id);
        IsLoading = false;
        OnPropertyChanged(nameof(NotFound));
        if (Detail is null) return;
        IsBookmarked = Detail.Summary.IsBookmarked;
        Skills.Clear();
        foreach (var s in Detail.Summary.Skills)
            Skills.Add(new SkillChip(s, mySkills.Contains(s, StringComparer.OrdinalIgnoreCase)));
        Notify();
    }

    void Notify()
    {
        foreach (var n in new[] { nameof(Row), nameof(CanApply), nameof(ApplyState), nameof(HasApplyState), nameof(MatchText), nameof(PembuatRating), nameof(Periode), nameof(Batas), nameof(BatasRelatif), nameof(HasLinkDetail) })
            OnPropertyChanged(n);
    }

    public bool NotFound => !IsLoading && Detail is null;
    public RekrutmenRow? Row => Detail is null ? null : new RekrutmenRow(Detail.Summary);
    public string Periode => Detail is null ? "" : Format.Range(Detail.Summary.TanggalMulai, Detail.Summary.TanggalSelesai);
    public string BatasRelatif => Detail is null ? "" : Format.DaysLeft(Detail.Summary.BatasPendaftaran);
    public string Batas => Detail is null ? "" : Format.Date(Detail.Summary.BatasPendaftaran);
    public string PembuatRating => Detail is null ? "" : Format.Rating(Detail.PembuatRating, Detail.PembuatUlasan);
    public string MatchText
    {
        get
        {
            var n = Skills.Count(s => s.Match);
            return n == 0 ? "Belum ada keahlian Anda yang cocok" : $"{n} dari {Skills.Count} keahlian cocok dengan profil Anda";
        }
    }

    public bool CanApply => Detail is { IsMine: false, StatusSaya: null } d && d.Summary.IsOpen && d.Summary.BatasPendaftaran.Date >= DateTime.Today;

    public string? ApplyState => Detail switch
    {
        null => null,
        { IsMine: true } => "Ini rekrutmen Anda. Kelola pelamar di Rekrutmen saya.",
        { StatusSaya: StatusPendaftar.Pending } => "Lamaran Anda sudah terkirim dan menunggu keputusan.",
        { StatusSaya: StatusPendaftar.Accepted } => "Anda diterima di tim ini.",
        { StatusSaya: StatusPendaftar.Rejected } => "Lamaran Anda untuk rekrutmen ini tidak diterima.",
        _ when !CanApply => "Pendaftaran untuk rekrutmen ini sudah ditutup.",
        _ => null,
    };
    public bool HasApplyState => ApplyState is not null;

    [RelayCommand]
    void Apply()
    {
        if (Detail is null) return;
        var sheet = _shell.Create<ApplySheetViewModel>(Detail.Summary, (Action)(() => _ = Load(Detail.Summary.Id, [.. Skills.Where(s => s.Match).Select(s => s.Name)])));
        _shell.OpenSheet(sheet);
    }

    [RelayCommand]
    async Task ToggleBookmark()
    {
        if (Detail is null) return;
        IsBookmarked = await _rekrutmen.ToggleBookmarkAsync(Detail.Summary.Id);
        _bookmarkChanged?.Invoke(IsBookmarked);
        _shell.ShowToast(IsBookmarked ? "Disimpan ke Tersimpan." : "Dihapus dari Tersimpan.");
    }

    [RelayCommand]
    async Task OpenContact()
    {
        if (Detail is null) return;
        var url = Detail.ContactPerson.StartsWith("http") ? Detail.ContactPerson : "https://" + Detail.ContactPerson;
        await _shell.OpenUrlAsync(url);
    }

    public bool HasLinkDetail => Detail?.LinkDetail is not null;

    [RelayCommand]
    async Task OpenLinkDetail()
    {
        if (Detail?.LinkDetail is { } url) await _shell.OpenUrlAsync(url);
    }

    [RelayCommand]
    void Manage()
    {
        if (Detail is not null) _shell.NavigateTo(Page.RekrutmenSaya, Detail.Summary.Id);
    }
}
