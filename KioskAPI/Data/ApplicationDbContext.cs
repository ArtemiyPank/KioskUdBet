using Microsoft.EntityFrameworkCore;
using KioskAPI.Models;

namespace KioskAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Table for application users
        public DbSet<User> Users { get; set; }

        // Table for storing refresh tokens
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Table for products available in the kiosk
        public DbSet<Product> Products { get; set; }

        // Table for customer orders
        public DbSet<Order> Orders { get; set; }

        // Table for items within each order
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-many: User → Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()                     // A user can have multiple orders
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many: Product → OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()                     // A product can appear in multiple order items
                .HasForeignKey(oi => oi.ProductId);

            // Configure one-to-one: User ↔ RefreshToken
            modelBuilder.Entity<User>()
                .HasOne(u => u.RefreshToken)
                .WithOne(rt => rt.User)
                .HasForeignKey<RefreshToken>(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
