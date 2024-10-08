using Microsoft.EntityFrameworkCore;

namespace Ratbags.Articles.API.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Articles__9C6270E87B1306A3");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.Published).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
