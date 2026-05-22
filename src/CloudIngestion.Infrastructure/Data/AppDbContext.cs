using CloudIngestion.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudIngestion.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FileRecord> FileRecords => Set<FileRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(128);
            entity.Property(e => e.ObjectKey).IsRequired().HasMaxLength(512);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.MetadataJson).HasColumnType("text");
            entity.Property(e => e.FailureReason).HasMaxLength(2048);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.UploadedAt);
        });
    }
}
