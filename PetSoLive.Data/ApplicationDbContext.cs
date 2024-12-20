using Microsoft.EntityFrameworkCore;
using PetSoLive.Core.Entities;

namespace PetSoLive.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor accepting DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Pet?> Pets { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Adoption> Adoptions { get; set; }
        public DbSet<Assistance> Assistances { get; set; }

        // Configure relationships if necessary
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.Pet)
                .WithMany() // Adjust if Pet has multiple Adoptions
                .HasForeignKey(a => a.PetId);

            modelBuilder.Entity<Adoption>()
                .HasOne(a => a.User)
                .WithMany() // Adjust if User has multiple Adoptions
                .HasForeignKey(a => a.UserId);
        }
    }
}