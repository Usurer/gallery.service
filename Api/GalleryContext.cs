using Microsoft.EntityFrameworkCore;

namespace Api;

public class GalleryContext : DbContext
{
    public GalleryContext()
    {
    }

    public GalleryContext(DbContextOptions<GalleryContext> options) : base(options)
    {
    }

    public virtual DbSet<Image> Images
    {
        get; set;
    }

#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=Gallery.db");

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