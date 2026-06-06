using Microsoft.EntityFrameworkCore;
using Luna.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace Luna.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<RecordAttendance> RecordAttendances { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<DateTime>()
                .HaveConversion<UtcDateTimeStringConverter>();

            configurationBuilder
                .Properties<DateTime?>()
                .HaveConversion<UtcDateTimeStringConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DiscordId)
                    .HasConversion(v => (long)v, v => (ulong)v)
                    .IsRequired();

                entity.HasIndex(e => e.DiscordId)
                    .IsUnique();

                entity.Property(e => e.Role)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });

            modelBuilder.Entity<Record>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ChannelId)
                    .HasConversion(v => (long)v, v => (ulong)v)
                    .IsRequired();

                entity.HasIndex(e => e.ChannelId)
                    .IsUnique();

                entity.HasOne(e => e.Executor)
                    .WithMany()
                    .HasForeignKey(e => e.ExecutorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                entity.HasMany(e => e.Attendances)
                    .WithOne(e => e.Record)
                    .HasForeignKey(e => e.RecordId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.Property(e => e.StartAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });

            modelBuilder.Entity<RecordAttendance>(entity =>
            {
                entity.ToTable("record_attendances");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.RecordId)
                    .IsRequired();

                entity.Property(e => e.DiscordUserId)
                    .HasConversion(v => (long)v, v => (ulong)v)
                    .IsRequired();

                entity.HasIndex(e => e.DiscordUserId);

                entity.HasIndex(e => new
                {
                    e.RecordId,
                    e.DiscordUserId
                });

                entity.Property(e => e.IsDeafened)
                    .IsRequired();

                entity.Property(e => e.StartAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd()
                    .IsRequired();

                entity.Property(e => e.EndAt)
                    .IsRequired(false);
            });
        }
    }
}

public class UtcDateTimeStringConverter : ValueConverter<DateTime, string>
{
    private const string Format = "yyyy-MM-dd HH:mm:ss";

    public UtcDateTimeStringConverter() : base(
        v => v.ToString(Format, CultureInfo.InvariantCulture),
        v => DateTime.SpecifyKind(
            DateTime.ParseExact(v, Format, CultureInfo.InvariantCulture), 
            DateTimeKind.Utc
        ))
    {
    }
}