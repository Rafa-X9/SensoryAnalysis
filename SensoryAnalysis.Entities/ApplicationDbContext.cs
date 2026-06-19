using Microsoft.EntityFrameworkCore;

namespace SensoryAnalysis.Entities;
public class ApplicationDbContext : DbContext
{
    public DbSet<Test> Tests { get; set; }
    public DbSet<Judger> Judgers { get; set; }
    public DbSet<Sample> Samples { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) 
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Test>().ToTable("Tests");
        modelBuilder.Entity<Judger>().ToTable("Judgers");
        modelBuilder.Entity<Sample>().ToTable("Samples");
    }
}