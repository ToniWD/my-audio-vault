using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicStream.Core.Entities;

namespace MusicStream.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<AudioFile> AudioFiles => Set<AudioFile>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Playlist> Playlists => Set<Playlist>();

    public AppDbContext(DbContextOptions<AppDbContext> contextOptions) : base(contextOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            // convert propertie to string (in db) from enum (in obj)
            entity.Property(u => u.Role)
                .HasConversion<string>();
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.HasOne(s => s.AudioFile)
                  .WithMany()
                  .HasForeignKey(s => s.AudioFileId)
                  .OnDelete(DeleteBehavior.Cascade); // If s.AudioFile is deleted then s will be deleted to
        });

        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasMany<Song>(p => p.Songs)
                .WithMany()
                .UsingEntity(j => j.ToTable("PlaylistSongs"));
        });

        modelBuilder.Entity<AudioFile>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.FilePath)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.HasIndex(a => a.FilePath)
                  .IsUnique();

            entity.Property(a => a.OriginalFileName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(a => a.ContentType)
                  .IsRequired()
                  .HasMaxLength(50);
        });
    }
}