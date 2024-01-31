using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public class GalleryContext : DbContext
{
    public GalleryContext(DbContextOptions<GalleryContext> options) : base(options)
    {
    }

    public virtual DbSet<FileSystemItem> FileSystemItems
    {
        get; set;
    }

    public virtual DbSet<ScanTarget> ScanTargets
    {
        get; set;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileSystemItem>(entity =>
        {
            entity.ToTable("FileSystemItems");

            entity.Property(e => e.Id).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.ParentId).HasColumnType("INTEGER");
            entity.Property(e => e.IsFolder);
            entity.Property(e => e.Path);
            entity.Property(e => e.Name);
            entity.Property(e => e.CreationDate);
            entity.Property(e => e.Extension);
        });

        modelBuilder.Entity<ScanTarget>(entity =>
        {
            entity.ToTable(nameof(ScanTargets));

            entity.Property(e => e.Id).HasColumnType("INTEGER").IsRequired();
            entity.Property(e => e.Path);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    private void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
    }
}