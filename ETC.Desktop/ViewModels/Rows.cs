using CommunityToolkit.Mvvm.ComponentModel;
using ETC.Desktop.Services;

namespace ETC.Desktop.ViewModels;

public partial class RekrutmenRow(RekrutmenSummary s) : ObservableObject
{
    public RekrutmenSummary Summary { get; } = s;
    public Guid Id => Summary.Id;
    public string Judul => Summary.Judul;
    public string Role => Summary.Role;
    public string KegiatanLabel => Labels.Of(Summary.Kegiatan);
    public string KegiatanIcon => Format.KegiatanIcon(Summary.Kegiatan);
    public string Pembuat => Summary.PembuatNama;
    public string PembuatNama => Summary.PembuatNama;
    public IReadOnlyList<string> Skills => Summary.Skills;
    public string DeadlineText => IsOpen ? Format.Deadline(Summary.BatasPendaftaran) : "Pendaftaran ditutup";
    public bool Urgent => Summary.IsOpen && Format.Urgent(Summary.BatasPendaftaran);
    public string SlotText => $"{Summary.Terisi} dari {Summary.Kuota} terisi";
    public double SlotProgress => Summary.Kuota == 0 ? 0 : 100.0 * Summary.Terisi / Summary.Kuota;
    public string FeeText => Format.Money(Summary.Fee);
    public string PendaftarText => Summary.JumlahPendaftar == 0 ? "Belum ada pelamar" : $"{Summary.JumlahPendaftar} pelamar";
    public int Menunggu => Summary.Menunggu;
    public bool HasMenunggu => Summary.Menunggu > 0;
    public bool IsOpen => Summary.IsOpen && Summary.BatasPendaftaran.Date >= DateTime.Today;
    public string StatusText => IsOpen ? "Dibuka" : "Ditutup";

    [ObservableProperty] private bool _isBookmarked = s.IsBookmarked;
}

public class SkillChip(string name, bool match)
{
    public string Name { get; } = name;
    public bool Match { get; } = match;
}
