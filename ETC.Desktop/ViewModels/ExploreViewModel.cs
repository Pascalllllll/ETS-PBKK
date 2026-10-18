using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Models;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public record SortOption(RekrutmenSort Value, string Label)
{
    public override string ToString() => Label;
}

public partial class ExploreViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly RekrutmenService _rekrutmen;
    private readonly bool _bookmarks;
    private CancellationTokenSource? _debounce;

    public ObservableCollection<RekrutmenRow> Items { get; } = new();
    public IReadOnlyList<SortOption> SortOptions { get; } =
    [
        new(RekrutmenSort.Terbaru, "Terbaru"),
        new(RekrutmenSort.TenggatTerdekat, "Tenggat terdekat"),
        new(RekrutmenSort.KuotaTerbanyak, "Sisa kuota terbanyak"),
    ];

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Kegiatan? _kegiatan;
    [ObservableProperty] private SortOption _sort;
    [ObservableProperty] private bool _onlyOpen = true;
    [ObservableProperty] private int _pageNumber = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalItems;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private RekrutmenRow? _selected;
    [ObservableProperty] private RekrutmenDetailViewModel? _detail;

    public bool IsBookmarks => _bookmarks;
    public bool IsExplore => !_bookmarks;
    public string Title => _bookmarks ? "Tersimpan" : "Jelajahi rekrutmen";
    public string Subtitle => _bookmarks
        ? "Rekrutmen yang Anda simpan, diurutkan dari tenggat terdekat."
        : "Lomba, riset, dan projek dari sesama mahasiswa.";
    public bool IsEmpty => !IsLoading && Error is null && Items.Count == 0;
    public string EmptyTitle => _bookmarks ? "Belum ada yang disimpan" : "Tidak ada rekrutmen yang cocok";
    public string EmptyBody => _bookmarks
        ? "Tekan Simpan pada rekrutmen yang menarik supaya mudah ditemukan lagi sebelum tenggatnya."
        : "Coba kata kunci lain, pilih Semua kegiatan, atau tampilkan juga rekrutmen yang sudah ditutup.";
    public string PageText => $"Halaman {PageNumber} dari {TotalPages}";
    public string CountText => TotalItems == 0 ? "" : $"{TotalItems} rekrutmen";
    public bool HasPaging => TotalPages > 1;

    public bool FilterAll { get => Kegiatan is null; set { if (value) Kegiatan = null; } }
    public bool FilterLomba { get => Kegiatan == Models.Kegiatan.Lomba; set { if (value) Kegiatan = Models.Kegiatan.Lomba; } }
    public bool FilterRiset { get => Kegiatan == Models.Kegiatan.Riset; set { if (value) Kegiatan = Models.Kegiatan.Riset; } }
    public bool FilterProjek { get => Kegiatan == Models.Kegiatan.Projek; set { if (value) Kegiatan = Models.Kegiatan.Projek; } }

    public ExploreViewModel(ShellViewModel shell, RekrutmenService rekrutmen, bool bookmarks, Guid focus)
    {
        _shell = shell;
        _rekrutmen = rekrutmen;
        _bookmarks = bookmarks;
        _sort = SortOptions[0];
        _ = LoadAsync(focus == Guid.Empty ? null : focus);
    }

    partial void OnSearchTextChanged(string value) => Debounced();
    partial void OnKegiatanChanged(Kegiatan? value)
    {
        OnPropertyChanged(nameof(FilterAll)); OnPropertyChanged(nameof(FilterLomba));
        OnPropertyChanged(nameof(FilterRiset)); OnPropertyChanged(nameof(FilterProjek));
        PageNumber = 1;
        _ = LoadAsync();
    }
    partial void OnSortChanged(SortOption value) { PageNumber = 1; _ = LoadAsync(); }
    partial void OnOnlyOpenChanged(bool value) { PageNumber = 1; _ = LoadAsync(); }

    partial void OnSelectedChanged(RekrutmenRow? value)
    {
        Detail = value is null ? null : _shell.Create<RekrutmenDetailViewModel>(value.Id,
            (Action<bool>)(b => value.IsBookmarked = b));
    }

    async void Debounced()
    {
        _debounce?.Cancel();
        var cts = _debounce = new CancellationTokenSource();
        try
        {
            await Task.Delay(250, cts.Token);
            PageNumber = 1;
            await LoadAsync();
        }
        catch (TaskCanceledException) { }
    }

    private int _loadVersion;

    async Task LoadAsync(Guid? focus = null)
    {
        // Filter bisa berubah beruntun; hanya hasil pemuatan terakhir yang dipakai.
        var version = ++_loadVersion;
        IsLoading = true;
        Error = null;
        try
        {
            List<RekrutmenSummary> list;
            if (_bookmarks)
            {
                list = await _rekrutmen.BookmarkedAsync();
                if (!string.IsNullOrWhiteSpace(SearchText))
                    list = list.Where(r => r.Judul.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                           || r.Role.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                if (Kegiatan is { } k) list = list.Where(r => r.Kegiatan == k).ToList();
                TotalItems = list.Count;
                TotalPages = 1;
            }
            else
            {
                var page = await _rekrutmen.SearchAsync(new RekrutmenQuery(SearchText, Kegiatan, OnlyOpen, Sort.Value, PageNumber, 12));
                // Item yang difokuskan dari halaman lain tetap tampil walau sudah ditutup.
                if (focus is { } f && page.Items.All(i => i.Id != f))
                {
                    var d = await _rekrutmen.GetDetailAsync(f);
                    list = d is null ? page.Items.ToList() : [d.Summary, .. page.Items];
                }
                else list = page.Items.ToList();
                TotalItems = page.TotalItems;
                TotalPages = page.TotalPages;
            }

            if (version != _loadVersion) return;
            Items.Clear();
            foreach (var r in list) Items.Add(new RekrutmenRow(r));
            Selected = focus is { } id ? Items.FirstOrDefault(i => i.Id == id) : Items.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Error = Format.Failure("Daftar rekrutmen", ex);
        }
        finally
        {
            if (version == _loadVersion) IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(PageText));
            OnPropertyChanged(nameof(CountText));
            OnPropertyChanged(nameof(HasPaging));
        }
    }

    [RelayCommand]
    Task Reload() => LoadAsync(Selected?.Id);

    [RelayCommand(CanExecute = nameof(CanPrev))]
    async Task PrevPage() { PageNumber--; await LoadAsync(); }
    bool CanPrev() => PageNumber > 1;

    [RelayCommand(CanExecute = nameof(CanNext))]
    async Task NextPage() { PageNumber++; await LoadAsync(); }
    bool CanNext() => PageNumber < TotalPages;

    partial void OnPageNumberChanged(int value) { PrevPageCommand.NotifyCanExecuteChanged(); NextPageCommand.NotifyCanExecuteChanged(); }
    partial void OnTotalPagesChanged(int value) { PrevPageCommand.NotifyCanExecuteChanged(); NextPageCommand.NotifyCanExecuteChanged(); }

    [RelayCommand]
    void ClearFilters()
    {
        SearchText = "";
        OnlyOpen = false;
        Kegiatan = null;
    }

    [RelayCommand]
    void GoExplore() => _shell.Navigate(Page.Jelajahi);
}
