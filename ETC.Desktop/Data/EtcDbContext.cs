using System.Text.Json;
using ETC.Desktop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ETC.Desktop.Data;

public class EtcDbContext(DbContextOptions<EtcDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Rekrutmen> Rekrutmens => Set<Rekrutmen>();
    public DbSet<Pendaftar> Pendaftars => Set<Pendaftar>();
    public DbSet<Tim> Tims => Set<Tim>();
    public DbSet<TimParticipant> TimParticipants => Set<TimParticipant>();
    public DbSet<History> Histories => Set<History>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<MentorRequest> MentorRequests => Set<MentorRequest>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // List<string> disimpan sebagai JSON, seperti di etc-be.
        var listConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
        var listComparer = new ValueComparer<List<string>>(
            (a, c) => a!.SequenceEqual(c!),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        b.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.NoPengenal).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
            e.Property(x => x.Spesialisasi).HasConversion(listConverter, listComparer);
        });

        b.Entity<Rekrutmen>(e =>
        {
            e.HasKey(x => x.RekrutmenId);
            e.Property(x => x.Kegiatan).HasConversion<string>();
            e.Property(x => x.Skills).HasConversion(listConverter, listComparer);
            e.HasOne(x => x.User).WithMany(u => u.Rekrutmens).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Tim).WithOne(t => t.Rekrutmen).HasForeignKey<Tim>(t => t.RekrutmenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Pendaftar>(e =>
        {
            e.HasKey(x => x.PendaftarId);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.RekrutmenId, x.UserId }).IsUnique();
            e.HasOne(x => x.Rekrutmen).WithMany(r => r.Pendaftar).HasForeignKey(x => x.RekrutmenId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.Lamaran).HasForeignKey(x => x.UserId);
        });

        b.Entity<Tim>(e =>
        {
            e.HasKey(x => x.TimId);
            e.Property(x => x.TipeTim).HasConversion<string>();
        });

        b.Entity<TimParticipant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TimId, x.UserId }).IsUnique();
            e.HasOne(x => x.Tim).WithMany(t => t.Participants).HasForeignKey(x => x.TimId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.TimParticipants).HasForeignKey(x => x.UserId);
        });

        b.Entity<History>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TimId, x.UserId, x.ReviewerUserId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.UlasanDiterima).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Reviewer).WithMany().HasForeignKey(x => x.ReviewerUserId);
            e.HasOne(x => x.Tim).WithMany(t => t.Histories).HasForeignKey(x => x.TimId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Bookmark>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RekrutmenId, x.UserId }).IsUnique();
            e.HasOne(x => x.Rekrutmen).WithMany(r => r.Bookmarks).HasForeignKey(x => x.RekrutmenId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany(u => u.Bookmarks).HasForeignKey(x => x.UserId);
        });

        b.Entity<MentorRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Kegiatan).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Mentor).WithMany().HasForeignKey(x => x.MentorId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });
    }
}
