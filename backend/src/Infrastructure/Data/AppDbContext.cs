using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicStream.Core.Entities;

namespace MusicStream.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> contextOptions) : base(contextOptions)
    {
    }

    public DbSet<Song> Songs => Set<Song>();
    public DbSet<AudioFile> AudioFiles => Set<AudioFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            // convert property to string (in db) from enum (in obj)
            entity.Property(u => u.Role)
                .HasConversion<string>();
        });

        // Config Song
        modelBuilder.Entity<Song>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.CoverImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(s => s.Genre)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasMany(s => s.AudioFiles)
                .WithOne(a => a.Song)
                .HasForeignKey(a => a.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Config AudioFile
        modelBuilder.Entity<AudioFile>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.AudioUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(a => a.Format)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(a => a.Quality)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(a => a.Checksum)
                .HasMaxLength(64);
        });
    }
}