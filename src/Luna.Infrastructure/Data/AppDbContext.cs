using Microsoft.EntityFrameworkCore;
using Luna.Domain.Entities;

namespace Luna.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        { }
        
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(users =>
            {
                users.HasKey(u => u.Id);
                users.Property(u => u.Id);
                users.HasIndex(u => u.DiscordId)
                    .IsUnique();
            });
        }
    }
}