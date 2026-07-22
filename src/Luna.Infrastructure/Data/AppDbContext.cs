using Microsoft.EntityFrameworkCore;
using Luna.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Luna.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<RecordAttendance> RecordAttendances { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventMember> EventMembers { get; set; }
        public DbSet<EventMemberEdit> EventMemberEdits { get; set; }
        public DbSet<EventAttendance> EventAttendances { get; set; }
        public DbSet<EventEdit> EventEdits { get; set; }
        public DbSet<EventEditExecutor> EventEditExecutors { get; set; }
        public DbSet<Executor> Executors { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<DateTime>()
                .HaveColumnType("timestamp without time zone")
                .HaveConversion<UtcDateTimeConverter>();

            configurationBuilder
                .Properties<DateTime?>()
                .HaveColumnType("timestamp without time zone")
                .HaveConversion<NullableUtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DiscordId)
                    .HasConversion(v => (long)v, v => (ulong)v)
                    .IsRequired(true);

                entity.HasIndex(e => e.DiscordId)
                    .IsUnique(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("timezone('utc', now())")
                    .ValueGeneratedOnAdd()
                    .IsRequired(true);
            });

            modelBuilder.Entity<Executor>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Executor>(e => e.UserId);

                entity.Property(e => e.Name)
                    .IsRequired(true);
                
                entity.Property(e => e.ImageUrl)
                    .IsRequired(false);

                entity.Property(e => e.Role)
                    .IsRequired(true);
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
                    .WithMany(e => e.Records)
                    .HasForeignKey(e => e.ExecutorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.Property(e => e.StartAt)
                    .HasDefaultValueSql("timezone('utc', now())")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });

            modelBuilder.Entity<RecordAttendance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RecordId)
                    .IsRequired();

                entity.Property(e => e.DiscordUserId)
                    .HasConversion(v => (long)v, v => (ulong)v)
                    .IsRequired();

                entity.HasOne(e => e.Record)
                    .WithMany(e => e.Attendances)
                    .HasForeignKey(e => e.RecordId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.HasIndex(e => e.DiscordUserId);
                
                entity.HasIndex(e => new { e.RecordId, e.DiscordUserId });

                entity.Property(e => e.IsDeafened)
                    .IsRequired();

                entity.Property(e => e.StartAt)
                    .HasDefaultValueSql("timezone('utc', now())")
                    .ValueGeneratedOnAdd()
                    .IsRequired();

                entity.Property(e => e.EndAt)
                    .IsRequired(false);
            });
        
            modelBuilder.Entity<EventType>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title)
                    .IsRequired();

                entity.HasIndex(e => e.Title)
                    .IsUnique();
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("timezone('utc', now())")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Creator)
                    .WithMany(e => e.Events)
                    .HasForeignKey(e => e.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                
                entity.HasOne(e => e.Type)
                    .WithMany(e => e.Events)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(e => e.TypeId)
                    .IsRequired();
                
                entity.Property(e => e.StartAt)
                    .IsRequired();
                
                entity.HasIndex(e => e.StartAt).HasMethod("brin");

                entity.Property(e => e.EndAt)
                    .IsRequired();
            });
        
            modelBuilder.Entity<EventMember>(entity =>
            {
                entity.HasKey(e => e.Id);
            
                entity.Property(e => e.Role)
                    .IsRequired();

                entity.HasOne(e => e.Event)
                    .WithMany(e => e.Members)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(e => e.EventId)
                    .IsRequired();
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired();
                
                entity.Property(e => e.IsActive)
                    .IsRequired();
            });
        
            modelBuilder.Entity<EventAttendance>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.IsDeafened)
                    .IsRequired();
                
                entity.Property(e => e.StartAt)
                    .IsRequired();

                entity.Property(e => e.EndAt)
                    .IsRequired();
                
                entity.HasOne(e => e.Member)
                    .WithMany(e => e.Attendances)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(e => e.MemberId)
                    .IsRequired();
            });
        
            modelBuilder.Entity<EventEdit>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EndCode)
                    .IsRequired();
                
                entity.HasOne(e => e.NewEventType)
                    .WithOne()
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasForeignKey<EventEdit>(e => e.NewTypeId)
                    .IsRequired(false);
                
                entity.HasOne(e => e.Event)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(e => e.EventId)
                    .IsRequired();
            });

            modelBuilder.Entity<EventMemberEdit>(entity =>
            {
                entity.HasKey(e => e.MemberId);

                entity.HasOne(e => e.Member)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey<EventMemberEdit>(e => e.MemberId)
                    .IsRequired();

                entity.HasOne(e => e.EventEdit)
                    .WithMany(e => e.MembersEdits)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(e => e.EventEditId)
                    .IsRequired();
                
                entity.Property(e => e.NewRole)
                    .IsRequired(false);
                
                entity.Property(e => e.NewActivityStatus)
                    .IsRequired(false);
            });
        
            modelBuilder.Entity<EventEditExecutor>(entity =>
            {
                entity.HasKey(e => new { e.EventEditId, e.ExecutorId });

                entity.HasOne(e => e.EventEdit)
                    .WithMany(e => e.EventEditExecutors)
                    .HasForeignKey(e => e.EventEditId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                
                entity.HasOne(e => e.Executor)
                    .WithMany(e => e.EventEditExecutors)
                    .HasForeignKey(e => e.ExecutorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("timezone('utc', now())")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
            });
        }
    }

    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() 
            : base(
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified), 
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
    }

    public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter() 
            : base(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : null, 
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null) { }
    }
}