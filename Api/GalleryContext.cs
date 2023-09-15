using Microsoft.EntityFrameworkCore;

namespace Api;

public class GalleryContext : DbContext
{
    private readonly IConfiguration Configuration;

    public GalleryContext(DbContextOptions<GalleryContext> options, IConfiguration configuration) : base(options)
    {
        Configuration = configuration;
    }

    public virtual DbSet<Image> Images
    {
        get; set;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(Configuration.GetConnectionString("sqlite"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("Images");

            entity.Property(e => e.Id).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Path);
            entity.Property(e => e.PreviewPath);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}