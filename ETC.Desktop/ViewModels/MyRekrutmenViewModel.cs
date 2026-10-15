using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public partial class ApplicantRow(ApplicantSummary a, IReadOnlyList<string> requested) : ObservableObject
{
    public ApplicantSummary Data { get; } = a;
    public string Nama => Data.Nama;
    public string Initials => Format.Initials(Data.Nama);
    public string Meta => $"{Data.Jurusan ?? "Jurusan belum diisi"} · {Format.Ago(Data.CreatedAt)}";
    public string StatusText => Labels.Of(Data.Status);
    public bool IsPending => Data.Status == StatusPendaftar.Pending;
    public bool IsAccepted => Data.Status == StatusPendaftar.Accepted;
    public bool IsRejected => Data.Status == StatusPendaftar.Rejected;
    public IReadOnlyList<SkillChip> Chips { get; } =
        a.Spesialisasi.Select(s => new SkillChip(s, requested.Contains(s, StringComparer.OrdinalIgnoreCase))).ToList();
    public int MatchCount => Chips.Count(c => c.Match);
    public string MatchText => requested.Count == 0 ? "" : $"{MatchCount} dari {requested.Count} keahlian cocok";
    public string Reputasi => Format.Rating(Data.Rating, Data.JumlahUlasan);
    public string Pengalaman => Data.TimSelesai == 0 ? "Belum pernah menyelesaikan kegiatan di ETC" : $"{Data.TimSelesai} kegiatan selesai di ETC";
    public string Alasan => string.IsNullOrWhiteSpace(Data.Alasan) ? "Pelamar tidak menulis alasan." : Data.Alasan;
    public string Kontak => Data.NoTelp;
    public string NoPengenal => Data.NoPengenal;
}

public partial class MyRekrutmenViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly RekrutmenService _rekrutmen;
    private readonly PendaftarService _pendaftar;

    public ObservableCollection<RekrutmenRow> Items { get; } = new();
    public ObservableCollection<ApplicantRow> Applicants { get; } = new();
    public ObservableCollection<ReviewItem> Reviews { get; } = new();

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private RekrutmenRow? _selected;
    [ObservableProperty] private ApplicantRow? _selectedApplicant;
    [ObservableProperty] private bool _applicantsLoading;

    public bool IsEmpty => !IsLoading && Error is null && Items.Count == 0;
    public bool HasSelected => Selected is not null;
    public bool NoApplicants => Selected is not null && !ApplicantsLoading && Applicants.Count == 0;
    public bool HasApplicant => SelectedApplicant is not null;
    public bool HasReviews => Reviews.Count > 0;
    public string ApplicantsHeader => Selected is null ? "" : $"{Applicants.Count} pelamar · {Applicants.Count(a => a.IsPending)} menunggu";

    public MyRekrutmenViewModel(ShellViewModel shell, RekrutmenService rekrutmen, PendaftarService pendaftar, Guid focus)
    {
        _shell = shell;
        _rekrutmen = rekrutmen;
        _pendaftar = pendaftar;
        _ = LoadAsync(focus == Guid.Empty ? null : focus);
    }

    async Task LoadAsync(Guid? focus = null)
    {
        IsLoading = true;
        Error = null;
        try
        {
            var list = await _rekrutmen.MineAsync();
            Items.Clear();
            foreach (var r in list) Items.Add(new RekrutmenRow(r));
            Selected = focus is { } f ? Items.FirstOrDefault(i => i.Id == f) ?? Items.FirstOrDefault()
                : Items.FirstOrDefault(i => i.HasMenunggu) ?? Items.FirstOrDefault();
        }
        catch (Exception ex) { Error = Format.Failure("Rekrutmen", ex); }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    private bool _suppressReload;

    partial void OnSelectedChanged(RekrutmenRow? value)
    {
        OnPropertyChanged(nameof(HasSelected));
        if (!_suppressReload) _ = LoadApplicantsAsync();
    }

    async Task LoadApplicantsAsync(Guid? keep = null)
    {
        Applicants.Clear();
        SelectedApplicant = null;
        if (Selected is null) { Notify(); return; }
        ApplicantsLoading = true;
        var list = await _pendaftar.ApplicantsAsync(Selected.Id);
        foreach (var a in list) Applicants.Add(new ApplicantRow(a, Selected.Skills));
        ApplicantsLoading = false;
        SelectedApplicant = (keep is { } k ? Applicants.FirstOrDefault(a => a.Data.PendaftarId == k) : null)
                            ?? Applicants.FirstOrDefault(a => a.IsPending) ?? Applicants.FirstOrDefault();
        Notify();
    }

    void Notify()
    {
        OnPropertyChanged(nameof(NoApplicants));
        OnPropertyChanged(nameof(ApplicantsHeader));
    }

    partial void OnSelectedApplicantChanged(ApplicantRow? value)
    {
        OnPropertyChanged(nameof(HasApplicant));
        _ = LoadReviewsAsync(value);
    }

    async Task LoadReviewsAsync(ApplicantRow? a)
    {
        Reviews.Clear();
        if (a is not null)
            foreach (var r in (await _pendaftar.ReviewsForAsync(a.Data.UserId)).Take(3)) Reviews.Add(r);
        OnPropertyChanged(nameof(HasReviews));
    }

    [RelayCommand]
    async Task Accept() => await Decide(true);

    [RelayCommand]
    async Task Reject() => await Decide(false);

    async Task Decide(bool accept)
    {
        if (SelectedApplicant is not { } a || Selected is null) return;
        var r = await _pendaftar.DecideAsync(a.Data.PendaftarId, accept);
        if (!r.Ok) { _shell.ShowToast(r.Error!); return; }
        _shell.ShowToast(accept ? $"{a.Nama} diterima dan masuk tim." : $"Lamaran {a.Nama} ditolak.");
        await RefreshSelected(a.Data.PendaftarId);
        await _shell.RefreshBadgesAsync();
    }

    async Task RefreshSelected(Guid? keepApplicant = null)
    {
        var id = Selected?.Id;
        var list = await _rekrutmen.MineAsync();
        Items.Clear();
        foreach (var r in list) Items.Add(new RekrutmenRow(r));
        // Pilih ulang tanpa memuat pelamar dua kali.
        _suppressReload = true;
        Selected = Items.FirstOrDefault(i => i.Id == id);
        _suppressReload = false;
        await LoadApplicantsAsync(keepApplicant);
    }

    [RelayCommand]
    void New() => _shell.OpenSheet(_shell.Create<RekrutmenFormViewModel>(Guid.Empty));

    [RelayCommand]
    void Edit()
    {
        if (Selected is not null) _shell.OpenSheet(_shell.Create<RekrutmenFormViewModel>(Selected.Id));
    }

    [RelayCommand]
    async Task ToggleOpen()
    {
        if (Selected is null) return;
        var open = !Selected.IsOpen;
        var r = await _rekrutmen.SetOpenAsync(Selected.Id, open);
        if (!r.Ok) { _shell.ShowToast(r.Error!); return; }
        _shell.ShowToast(open ? "Pendaftaran dibuka kembali." : "Pendaftaran ditutup. Pelamar baru tidak bisa mendaftar.");
        await RefreshSelected(SelectedApplicant?.Data.PendaftarId);
    }

    [RelayCommand]
    void Delete()
    {
        if (Selected is not { } s) return;
        _shell.OpenSheet(new ConfirmSheetViewModel(
            $"Hapus “{s.Judul}”?",
            $"Rekrutmen, {s.Summary.JumlahPendaftar} lamaran, dan data timnya akan dihapus permanen. Tindakan ini tidak bisa dibatalkan.",
            "Hapus rekrutmen", true,
            async () =>
            {
                var r = await _rekrutmen.DeleteAsync(s.Id);
                if (r.Ok)
                {
                    _shell.ShowToast("Rekrutmen dihapus.");
                    await LoadAsync();
                    await _shell.RefreshBadgesAsync();
                }
                return r;
            }));
    }

    [RelayCommand]
    Task OpenLink(string url) => _shell.OpenUrlAsync(url);

    [RelayCommand]
    async Task OpenWhatsApp()
    {
        if (SelectedApplicant is { } a) await _shell.OpenUrlAsync(Format.WhatsAppUrl(a.Kontak));
    }
}
