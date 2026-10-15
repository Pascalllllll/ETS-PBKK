namespace ETC.Desktop.Models;

// Skema mengikuti etc-be/internal/models, ditambah kolom khusus versi desktop.
public enum UserRole { Mahasiswa, Dosen }

public enum Kegiatan { Lomba, Riset, Projek }

public enum StatusPendaftar { Pending, Accepted, Rejected }

public class User
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Nama { get; set; } = "";
    public string? Jurusan { get; set; }
    public string NoPengenal { get; set; } = "";
    public UserRole Role { get; set; }
    public string NoTelp { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Bio { get; set; }
    public List<string> Spesialisasi { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<Rekrutmen> Rekrutmens { get; set; } = new();
    public List<Pendaftar> Lamaran { get; set; } = new();
    public List<Bookmark> Bookmarks { get; set; } = new();
    public List<TimParticipant> TimParticipants { get; set; } = new();
    public List<History> UlasanDiterima { get; set; } = new();
}

public class Rekrutmen
{
    public Guid RekrutmenId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Kegiatan Kegiatan { get; set; }
    public string Judul { get; set; } = "";
    public string Role { get; set; } = "";
    public string Deskripsi { get; set; } = "";
    public string Kriteria { get; set; } = "";
    public List<string> Skills { get; set; } = new();
    public int Kuota { get; set; } = 1;
    public DateTime BatasPendaftaran { get; set; }
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalSelesai { get; set; }
    public decimal? Fee { get; set; }
    public string ContactPerson { get; set; } = "";
    public bool IsOpen { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User User { get; set; } = null!;
    public List<Pendaftar> Pendaftar { get; set; } = new();
    public List<Bookmark> Bookmarks { get; set; } = new();
    public Tim? Tim { get; set; }
}

public class Pendaftar
{
    public Guid PendaftarId { get; set; } = Guid.NewGuid();
    public Guid RekrutmenId { get; set; }
    public Guid UserId { get; set; }
    public string? AlasanMendaftar { get; set; }
    public string CvUrl { get; set; } = "";
    public string PortofolioUrl { get; set; } = "";
    public StatusPendaftar Status { get; set; } = StatusPendaftar.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DecidedAt { get; set; }

    public Rekrutmen Rekrutmen { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class Tim
{
    public Guid TimId { get; set; } = Guid.NewGuid();
    public Guid RekrutmenId { get; set; }
    public Kegiatan TipeTim { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Rekrutmen Rekrutmen { get; set; } = null!;
    public List<TimParticipant> Participants { get; set; } = new();
    public List<History> Histories { get; set; } = new();
}

public class TimParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TimId { get; set; }
    public Guid UserId { get; set; }
    public int MemberKe { get; set; }

    public Tim Tim { get; set; } = null!;
    public User User { get; set; } = null!;
}

// Ulasan antar anggota tim (tabel histories di etc-be).
public class History
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public Guid TimId { get; set; }
    public int Rating { get; set; }
    public string Deskripsi { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User User { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
    public Tim Tim { get; set; } = null!;
}

public class Bookmark
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RekrutmenId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Rekrutmen Rekrutmen { get; set; } = null!;
    public User User { get; set; } = null!;
}
