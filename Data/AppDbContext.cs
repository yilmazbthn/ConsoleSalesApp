using ConsoleSalesApp.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleSalesApp.Data;

public class AppDbContext:DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderHistory> OrderHistories { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=localhost;Database=ConsoleSalesApp;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Order - User ilişkisi
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order - SalesPerson ilişkisi
        modelBuilder.Entity<Order>()
            .HasOne(o => o.SalesPerson)
            .WithMany()
            .HasForeignKey(o => o.SalesPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // OrderDetail - Order ilişkisi
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderDetail - OrderItem ilişkisi
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.OrderItem)
            .WithMany()
            .HasForeignKey(od => od.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }


}

