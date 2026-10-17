using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ETC.Desktop.ViewModels;

public enum Page { Ringkasan, Jelajahi, Tersimpan, RekrutmenSaya, LamaranSaya, TimSaya, Mentor, Profil }

public partial class ShellViewModel : ViewModelBase
{
    private readonly IServiceProvider _sp;
    private readonly Session _session;
    private readonly DashboardService _dashboard;
    private CancellationTokenSource? _toastCts;

    [ObservableProperty] private Page _page;
    [ObservableProperty] private ViewModelBase? _content;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasSheet))] private SheetViewModel? _sheet;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasToast))] private string? _toast;
    [ObservableProperty] private bool _sidebarVisible = true;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasPelamarMenunggu))] private int _pelamarMenunggu;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasBimbinganMenunggu))] private int _bimbinganMenunggu;

    public bool HasSheet => Sheet is not null;
    public bool HasToast => Toast is not null;
    public bool HasPelamarMenunggu => PelamarMenunggu > 0;
    public bool HasBimbinganMenunggu => BimbinganMenunggu > 0;

    public string Nama => _session.CurrentUser?.Nama ?? "";
    public string? FotoPath => _session.CurrentUser?.FotoPath;
    public string RoleText => _session.CurrentUser is { } u ? $"{Labels.Of(u.Role)} · {u.NoPengenal}" : "";

    public ShellViewModel(IServiceProvider sp, Session session, DashboardService dashboard)
    {
        _sp = sp;
        _session = session;
        _dashboard = dashboard;
        session.Changed += () =>
        {
            OnPropertyChanged(nameof(Nama));
            OnPropertyChanged(nameof(FotoPath));
            OnPropertyChanged(nameof(RoleText));
        };
        // Page sudah Ringkasan sejak awal, jadi setter tidak terpicu.
        Load(Page.Ringkasan, null);
    }

    public T Create<T>(params object[] args) where T : notnull => ActivatorUtilities.CreateInstance<T>(_sp, [this, .. args]);

    private bool _suppressLoad;

    partial void OnPageChanged(Page value)
    {
        if (!_suppressLoad) Load(value, null);
    }

    [RelayCommand]
    public void Navigate(Page page)
    {
        if (Page == page && Content is not null) { Load(page, null); return; }
        Page = page;
    }

    public void NavigateTo(Page page, Guid focusId)
    {
        _suppressLoad = true;
        Page = page;
        _suppressLoad = false;
        Load(page, focusId);
    }

    void Load(Page page, Guid? focus)
    {
        Content = page switch
        {
            Page.Ringkasan => Create<DashboardViewModel>(),
            Page.Jelajahi => Create<ExploreViewModel>(false, focus ?? Guid.Empty),
            Page.Tersimpan => Create<ExploreViewModel>(true, focus ?? Guid.Empty),
            Page.RekrutmenSaya => Create<MyRekrutmenViewModel>(focus ?? Guid.Empty),
            Page.LamaranSaya => Create<MyApplicationsViewModel>(),
            Page.TimSaya => Create<TeamsViewModel>(),
            Page.Mentor => Create<MentorViewModel>(MentorTab.Daftar),
            _ => Create<ProfileViewModel>(),
        };
        _ = RefreshBadgesAsync();
    }

    public async Task RefreshBadgesAsync()
    {
        try { PelamarMenunggu = (await _dashboard.LoadAsync()).PelamarMenunggu; }
        catch (Exception) { PelamarMenunggu = 0; }
        try { BimbinganMenunggu = await _sp.GetRequiredService<MentorService>().PendingIncomingCountAsync(); }
        catch (Exception) { BimbinganMenunggu = 0; }
    }

    public void OpenSheet(SheetViewModel sheet)
    {
        sheet.Close = () => { if (Sheet == sheet) Sheet = null; };
        Sheet = sheet;
    }

    [RelayCommand]
    void CloseSheet() => Sheet?.Close?.Invoke();

    public async void ShowToast(string message)
    {
        _toastCts?.Cancel();
        var cts = _toastCts = new CancellationTokenSource();
        Toast = message;
        try
        {
            await Task.Delay(3200, cts.Token);
            Toast = null;
        }
        catch (TaskCanceledException) { }
    }

    static Avalonia.Controls.TopLevel? TopLevel =>
        Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d
            ? Avalonia.Controls.TopLevel.GetTopLevel(d.MainWindow) : null;

    public async Task<Avalonia.Platform.Storage.IStorageFile?> PickImageAsync(string title)
    {
        if (TopLevel is not { } top) return null;
        var files = await top.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [new Avalonia.Platform.Storage.FilePickerFileType("Gambar") { Patterns = ["*.jpg", "*.jpeg", "*.png", "*.webp"] }],
        });
        return files.FirstOrDefault();
    }

    public async Task OpenUrlAsync(string url)
    {
        var top = TopLevel;
        var ok = top is not null && Uri.TryCreate(url, UriKind.Absolute, out var uri) && await top.Launcher.LaunchUriAsync(uri);
        if (!ok) ShowToast($"Tidak bisa membuka {url}");
    }

    [RelayCommand]
    void NewRekrutmen()
    {
        var form = Create<RekrutmenFormViewModel>(Guid.Empty);
        OpenSheet(form);
    }

    [RelayCommand]
    void ToggleSidebar() => SidebarVisible = !SidebarVisible;

    [RelayCommand]
    void Logout() => _sp.GetRequiredService<AuthService>().Logout();
}
