using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public enum MentorTab { Daftar, Terkirim, Masuk }

public class MentorCard(MentorSummary m)
{
    public MentorSummary Data { get; } = m;
    public string Nama => Data.Nama;
    public string? FotoPath => Data.FotoPath;
    public string Subtitle => $"{Data.Jurusan ?? "Mahasiswa ITS"}{(Data.Angkatan is { } a ? $", angkatan {a}" : "")}";
    public IReadOnlyList<string> Skills => Data.Skills;
    public IReadOnlyList<string> Pengalaman { get; } =
        m.Pengalaman.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(4).ToList();
    public string Reputasi => $"{Format.Rating(Data.Rating, Data.JumlahUlasan)} · " +
                              (Data.Dibimbing == 0 ? "belum ada bimbingan" : $"{Data.Dibimbing} bimbingan diterima");
    public bool HasLinkedIn => Data.LinkedInUrl is not null;
    public bool HasPortofolio => Data.PortofolioUrl is not null;
}

public class MentorRequestRow(MentorRequestItem r, bool incoming)
{
    public MentorRequestItem Data { get; } = r;
    public bool Incoming { get; } = incoming;
    public string Nama => Incoming ? Data.UserNama : Data.MentorNama;
    public string? FotoPath => Incoming ? Data.UserFoto : Data.MentorFoto;
    public string Topik => Data.Topik;
    public string Pesan => Data.Pesan;
    public string Meta => Incoming
        ? $"{Labels.Of(Data.Kegiatan)} · {Data.UserJurusan ?? "Jurusan belum diisi"} · dikirim {Format.Ago(Data.CreatedAt)}"
        : $"{Labels.Of(Data.Kegiatan)} · dikirim {Format.Ago(Data.CreatedAt)}";
    public string StatusText => Labels.Of(Data.Status);
    public bool IsPending => Data.Status == StatusBimbingan.Pending;
    public bool IsAccepted => Data.Status == StatusBimbingan.Accepted;
    public bool IsRejected => Data.Status == StatusBimbingan.Rejected;
    public bool CanCancel => !Incoming && IsPending;
    public bool CanDecide => Incoming && IsPending;
}

public partial class MentorViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly MentorService _service;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsDaftar), nameof(IsTerkirim), nameof(IsMasuk))] private MentorTab _tab;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _error;

    public ObservableCollection<MentorCard> Mentors { get; } = new();
    public ObservableCollection<MentorRequestRow> Terkirim { get; } = new();
    public ObservableCollection<MentorRequestRow> Masuk { get; } = new();

    public bool IsMentor { get; }
    public bool IsDaftar { get => Tab == MentorTab.Daftar; set { if (value) Tab = MentorTab.Daftar; } }
    public bool IsTerkirim { get => Tab == MentorTab.Terkirim; set { if (value) Tab = MentorTab.Terkirim; } }
    public bool IsMasuk { get => Tab == MentorTab.Masuk; set { if (value) Tab = MentorTab.Masuk; } }

    public bool MentorsEmpty => !IsLoading && Error is null && Mentors.Count == 0;
    public bool TerkirimEmpty => !IsLoading && Terkirim.Count == 0;
    public bool MasukEmpty => !IsLoading && Masuk.Count == 0;
    public string CountText => Mentors.Count == 0 ? "" : $"{Mentors.Count} mentor";
    public int MasukMenunggu => Masuk.Count(m => m.IsPending);
    public string TerkirimLabel => Terkirim.Count == 0 ? "Permintaan saya" : $"Permintaan saya ({Terkirim.Count})";
    public string MasukLabel => MasukMenunggu == 0 ? "Permintaan masuk" : $"Permintaan masuk ({MasukMenunggu})";

    public MentorViewModel(ShellViewModel shell, MentorService service, Session session, MentorTab tab)
    {
        _shell = shell;
        _service = service;
        IsMentor = session.CurrentUser?.IsMentor ?? false;
        _tab = tab == MentorTab.Masuk && !IsMentor ? MentorTab.Daftar : tab;
        _ = LoadAsync();
    }

    async partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        var cts = _searchCts = new CancellationTokenSource();
        try { await Task.Delay(250, cts.Token); }
        catch (TaskCanceledException) { return; }
        await LoadMentorsAsync();
    }

    [RelayCommand]
    async Task LoadAsync()
    {
        IsLoading = true;
        Error = null;
        try
        {
            await LoadMentorsAsync();
            await LoadRequestsAsync();
        }
        catch (Exception ex) { Error = Format.Failure("Daftar mentor", ex); }
        finally
        {
            IsLoading = false;
            NotifyLists();
        }
    }

    async Task LoadMentorsAsync()
    {
        var list = await _service.ListAsync(SearchText);
        Mentors.Clear();
        foreach (var m in list) Mentors.Add(new MentorCard(m));
        NotifyLists();
    }

    async Task LoadRequestsAsync()
    {
        Terkirim.Clear();
        foreach (var r in await _service.SentAsync()) Terkirim.Add(new MentorRequestRow(r, false));
        Masuk.Clear();
        if (IsMentor)
            foreach (var r in await _service.IncomingAsync()) Masuk.Add(new MentorRequestRow(r, true));
        NotifyLists();
    }

    void NotifyLists()
    {
        foreach (var n in new[] { nameof(MentorsEmpty), nameof(TerkirimEmpty), nameof(MasukEmpty), nameof(CountText),
                     nameof(MasukMenunggu), nameof(TerkirimLabel), nameof(MasukLabel) })
            OnPropertyChanged(n);
    }

    [RelayCommand]
    void ClearFilters() => SearchText = "";

    [RelayCommand]
    void NeedMentor(MentorCard? card)
    {
        var sheet = _shell.Create<MentorRequestSheetViewModel>(card?.Data.UserId ?? Guid.Empty, (Func<Task>)(async () =>
        {
            await LoadRequestsAsync();
            Tab = MentorTab.Terkirim;
        }));
        _shell.OpenSheet(sheet);
    }

    [RelayCommand]
    async Task OpenLinkedIn(MentorCard card)
    {
        if (card.Data.LinkedInUrl is { } u) await _shell.OpenUrlAsync(u);
    }

    [RelayCommand]
    async Task OpenPortofolio(MentorCard card)
    {
        if (card.Data.PortofolioUrl is { } u) await _shell.OpenUrlAsync(u);
    }

    [RelayCommand]
    async Task Contact(MentorRequestRow row) =>
        await _shell.OpenUrlAsync(Format.WhatsAppUrl(row.Incoming ? row.Data.UserNoTelp : row.Data.MentorNoTelp));

    [RelayCommand]
    async Task Accept(MentorRequestRow row) => await Decide(row, true);

    [RelayCommand]
    async Task Reject(MentorRequestRow row) => await Decide(row, false);

    async Task Decide(MentorRequestRow row, bool accept)
    {
        var r = await _service.DecideAsync(row.Data.Id, accept);
        if (!r.Ok) { _shell.ShowToast(r.Error!); return; }
        _shell.ShowToast(accept ? $"Bimbingan untuk {row.Nama} diterima. Hubungi lewat WhatsApp." : $"Permintaan {row.Nama} ditolak.");
        await LoadRequestsAsync();
        await _shell.RefreshBadgesAsync();
    }

    [RelayCommand]
    void Cancel(MentorRequestRow row)
    {
        _shell.OpenSheet(new ConfirmSheetViewModel("Batalkan permintaan?",
            $"Permintaan bimbingan \"{row.Topik}\" ke {row.Nama} akan dihapus.", "Batalkan permintaan", true,
            async () =>
            {
                var r = await _service.CancelAsync(row.Data.Id);
                if (r.Ok)
                {
                    await LoadRequestsAsync();
                    _shell.ShowToast("Permintaan dibatalkan.");
                }
                return r;
            }));
    }

    [RelayCommand]
    void EditProfile() => _shell.Navigate(Page.Profil);
}

public record MentorOption(Guid Id, string Nama, string Keterangan)
{
    public override string ToString() => $"{Nama} · {Keterangan}";
}

public partial class MentorRequestSheetViewModel : SheetViewModel
{
    private readonly ShellViewModel _shell;
    private readonly MentorService _service;
    private readonly Func<Task> _onDone;

    public ObservableCollection<MentorOption> Mentors { get; } = new();
    [ObservableProperty] private MentorOption? _mentor;
    [ObservableProperty] private Kegiatan _kegiatan = Kegiatan.Lomba;
    [ObservableProperty] private string _topik = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PesanCount))] private string _pesan = "";
    public string PesanCount => $"{Pesan.Length}/800";

    public bool IsLomba { get => Kegiatan == Kegiatan.Lomba; set { if (value) Kegiatan = Kegiatan.Lomba; } }
    public bool IsRiset { get => Kegiatan == Kegiatan.Riset; set { if (value) Kegiatan = Kegiatan.Riset; } }
    public bool IsProjek { get => Kegiatan == Kegiatan.Projek; set { if (value) Kegiatan = Kegiatan.Projek; } }

    partial void OnKegiatanChanged(Kegiatan value)
    {
        OnPropertyChanged(nameof(IsLomba)); OnPropertyChanged(nameof(IsRiset)); OnPropertyChanged(nameof(IsProjek));
    }

    public MentorRequestSheetViewModel(ShellViewModel shell, MentorService service, Guid mentorId, Func<Task> onDone)
    {
        _shell = shell;
        _service = service;
        _onDone = onDone;
        _ = LoadAsync(mentorId);
    }

    async Task LoadAsync(Guid mentorId)
    {
        foreach (var m in await _service.ListAsync())
            Mentors.Add(new MentorOption(m.UserId, m.Nama, string.Join(", ", m.Skills.Take(2))));
        Mentor = Mentors.FirstOrDefault(m => m.Id == mentorId);
    }

    [RelayCommand]
    async Task Submit()
    {
        var input = new MentorRequestInput(Mentor?.Id ?? Guid.Empty, Kegiatan, Topik, Pesan);
        Error = MentorService.Validate(input);
        if (Error is not null) return;
        IsBusy = true;
        var r = await _service.RequestAsync(input);
        IsBusy = false;
        if (!r.Ok) { Error = r.Error; return; }
        Close?.Invoke();
        _shell.ShowToast($"Permintaan terkirim ke {Mentor!.Nama}. Pantau statusnya di Permintaan saya.");
        await _onDone();
    }
}
