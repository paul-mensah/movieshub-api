using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MoviesHub.Api.Storage.Entities;

namespace MoviesHub.Api.Storage;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<FavoriteMovie> FavoriteMovies { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")).Build();
        
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        string connectionString = configuration.GetConnectionString("DbConnection");
        builder.UseNpgsql(connectionString);
        return new ApplicationDbContext(builder.Options);
    }
}