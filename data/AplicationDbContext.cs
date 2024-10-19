using BookLibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLibraryAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasOne(b => b.User) 
                .WithMany(u => u.Books) 
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}