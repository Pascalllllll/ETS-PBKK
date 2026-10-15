using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public partial class MemberRow(TimMember m, TimSummary tim) : ObservableObject
{
    public TimMember Data { get; } = m;
    public TimSummary Tim { get; } = tim;
    public string Nama => Data.IsMe ? $"{Data.Nama} (Anda)" : Data.Nama;
    public string Initials => Format.Initials(Data.Nama);
    public string Peran => Data.MemberKe == 1 ? "Ketua" : $"Anggota ke-{Data.MemberKe}";
    public string Meta => $"{Labels.Of(Data.Role)} · {Data.Jurusan ?? "Jurusan belum diisi"}";
    public IReadOnlyList<string> Skills => Data.Spesialisasi;
    public bool CanRate => !Data.IsMe && Data.RatingDariSaya is null && Tim.Selesai;
    public bool Rated => Data.RatingDariSaya is not null;
    public string RatedText => Data.RatingDariSaya is { } r ? $"Anda memberi {r} dari 5" : "";
    public bool WaitingForEnd => !Data.IsMe && Data.RatingDariSaya is null && !Tim.Selesai;
}

public class TeamRow(TimSummary t)
{
    public TimSummary Data { get; } = t;
    public string Judul => Data.Judul;
    public string KegiatanLabel => Labels.Of(Data.Kegiatan);
    public string KegiatanIcon => Format.KegiatanIcon(Data.Kegiatan);
    public string Peran => Data.SayaKetua ? "Anda ketua" : "Anda anggota";
    public string Periode => Format.Range(Data.TanggalMulai, Data.TanggalSelesai);
    public string StatusText => Data.Selesai ? "Selesai" : Data.Berjalan ? "Berjalan" : "Belum mulai";
    public bool IsDone => Data.Selesai;
    public string MemberCount => $"{Data.Members.Count} orang";
    public IReadOnlyList<MemberRow> Members { get; } = t.Members.Select(m => new MemberRow(m, t)).ToList();
}

public partial class TeamsViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private readonly TimService _tim;

    public ObservableCollection<TeamRow> Items { get; } = new();
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _error;
    [ObservableProperty] private TeamRow? _selected;

    public bool IsEmpty => !IsLoading && Error is null && Items.Count == 0;

    public TeamsViewModel(ShellViewModel shell, TimService tim)
    {
        _shell = shell;
        _tim = tim;
        _ = LoadAsync();
    }

    async Task LoadAsync(Guid? keep = null)
    {
        IsLoading = true;
        Error = null;
        try
        {
            var list = await _tim.MyTeamsAsync();
            Items.Clear();
            foreach (var t in list) Items.Add(new TeamRow(t));
            Selected = Items.FirstOrDefault(i => i.Data.TimId == keep) ?? Items.FirstOrDefault();
        }
        catch (Exception ex) { Error = Format.Failure("Tim", ex); }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    void Rate(MemberRow m) => _shell.OpenSheet(_shell.Create<RatingSheetViewModel>(m,
        (Func<Task>)(() => LoadAsync(m.Tim.TimId))));

    [RelayCommand]
    Task Contact(MemberRow m) => _shell.OpenUrlAsync(Format.WhatsAppUrl(m.Data.NoTelp));

    [RelayCommand]
    void Explore() => _shell.Navigate(Page.Jelajahi);
}
