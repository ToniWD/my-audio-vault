using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicStream.Infrastructure.Data;

public class AppContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=music_stream_db;Username=postgres;Password=mypassword");

        return new AppDbContext(optionsBuilder.Options);
    }
}