using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public class ApplicationRow(MyApplication a)
{
    public MyApplication Data { get; } = a;
    public RekrutmenRow Rekrutmen { get; } = new(a.Rekrutmen);
    public string StatusText => Labels.Of(Data.Status);
    public bool IsPending => Data.Status == StatusPendaftar.Pending;
    public bool IsAccepted => Data.Status == StatusPendaftar.Accepted;
    public bool IsRejected => Data.Status == StatusPendaftar.Rejected;
    public string Timeline => Data.DecidedAt is { } d
        ? $"Dikirim {Format.Date(Data.CreatedAt)} · diputuskan {Format.Date(d)}"
        : $"Dikirim {Format.Ago(Data.CreatedAt)} · {char.ToLower(Rekrutmen.DeadlineText[0]) + Rekrutmen.DeadlineText[1..]}";
}

public partial class MyApplicationsViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly PendaftarService _pendaftar;

    public ObservableCollection<ApplicationRow> Items { get; } = new();
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private StatusCount? _counts;

    public bool IsEmpty => !IsLoading && Error is null && Items.Count == 0;
    public string Summary => Counts is null ? "" : $"{Counts.Menunggu} menunggu · {Counts.Diterima} diterima · {Counts.Ditolak} ditolak";

    public MyApplicationsViewModel(ShellViewModel shell, PendaftarService pendaftar)
    {
        _shell = shell;
        _pendaftar = pendaftar;
        _ = LoadAsync();
    }

    async Task LoadAsync()
    {
        IsLoading = true;
        Error = null;
        try
        {
            var list = await _pendaftar.MyApplicationsAsync();
            Items.Clear();
            foreach (var a in list) Items.Add(new ApplicationRow(a));
            Counts = new StatusCount(list.Count(a => a.Status == StatusPendaftar.Pending),
                list.Count(a => a.Status == StatusPendaftar.Accepted), list.Count(a => a.Status == StatusPendaftar.Rejected));
        }
        catch (Exception ex) { Error = Format.Failure("Lamaran", ex); }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(Summary));
        }
    }

    [RelayCommand]
    void Withdraw(ApplicationRow row) => _shell.OpenSheet(new ConfirmSheetViewModel(
        "Batalkan lamaran?",
        $"Lamaran Anda untuk “{row.Rekrutmen.Judul}” akan dihapus. Anda bisa melamar lagi selama pendaftaran masih dibuka.",
        "Batalkan lamaran", true,
        async () =>
        {
            var r = await _pendaftar.WithdrawAsync(row.Data.PendaftarId);
            if (r.Ok) { _shell.ShowToast("Lamaran dibatalkan."); await LoadAsync(); }
            return r;
        }));

    [RelayCommand]
    void Open(ApplicationRow row) => _shell.NavigateTo(Page.Jelajahi, row.Rekrutmen.Id);

    [RelayCommand]
    void Explore() => _shell.Navigate(Page.Jelajahi);
}
