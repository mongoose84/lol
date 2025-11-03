using Microsoft.EntityFrameworkCore;
using RiotProxy.Domain;
using RiotProxy.Infrastructure;

namespace RiotProxy.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // parameterless ctor used by some tools (optional)
    public ApplicationDbContext() { }

    public DbSet<Person> Persons { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var conn = Secrets.DatabaseConnectionString;
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("Database connection string is not configured. Call Secrets.Initialize() before constructing the DbContext.");

            optionsBuilder.UseSqlServer(conn);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(b =>
        {
            b.ToTable("User");
            b.HasKey(p => p.UserId);
            b.Property(p => p.UserName).IsRequired().HasMaxLength(200);
        });

        base.OnModelCreating(modelBuilder);
    }
}