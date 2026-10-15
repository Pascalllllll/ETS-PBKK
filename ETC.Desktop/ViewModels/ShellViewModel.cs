using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ETC.Desktop.ViewModels;

public enum Page { Ringkasan, Jelajahi, Tersimpan, RekrutmenSaya, LamaranSaya, TimSaya, Profil }

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

    public bool HasSheet => Sheet is not null;
    public bool HasToast => Toast is not null;
    public bool HasPelamarMenunggu => PelamarMenunggu > 0;

    public string Nama => _session.CurrentUser?.Nama ?? "";
    public string Initials => Format.Initials(Nama);
    public string RoleText => _session.CurrentUser is { } u ? $"{Labels.Of(u.Role)} · {u.NoPengenal}" : "";

    public ShellViewModel(IServiceProvider sp, Session session, DashboardService dashboard)
    {
        _sp = sp;
        _session = session;
        _dashboard = dashboard;
        session.Changed += () =>
        {
            OnPropertyChanged(nameof(Nama));
            OnPropertyChanged(nameof(Initials));
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
            _ => Create<ProfileViewModel>(),
        };
        _ = RefreshBadgesAsync();
    }

    public async Task RefreshBadgesAsync()
    {
        try { PelamarMenunggu = (await _dashboard.LoadAsync()).PelamarMenunggu; }
        catch (Exception) { PelamarMenunggu = 0; }
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

    public async Task OpenUrlAsync(string url)
    {
        var top = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime d
            ? Avalonia.Controls.TopLevel.GetTopLevel(d.MainWindow) : null;
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
