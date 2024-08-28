using Microsoft.EntityFrameworkCore;
using KioskAPI.Models;

namespace KioskAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связи между Order и User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany() // У пользователя может быть много заказов
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация для OrderItem и Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany() // Предполагается, что у Product может быть много OrderItems
                .HasForeignKey(oi => oi.ProductId);

            // Настройка связи один-к-одному между User и RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id); // Установка Id как первичного ключа

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithOne(u => u.RefreshToken)
                .HasForeignKey<RefreshToken>(rt => rt.UserId); // Определение внешнего ключа и связи один-к-одному между User и RefreshToken
        }
    }
}
