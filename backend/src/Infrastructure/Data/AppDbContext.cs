using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Domain.Models;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

    
        public AppDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; } 

  
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                Configuration.GetConnectionString("DefaultConnection"),  
                b => b.MigrationsAssembly("Infrastructure")
            );
        }

   
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)  
                .OnDelete(DeleteBehavior.Cascade);  

            modelBuilder.Entity<OrderItem>()
                .ToTable("OrderItems"); 

        }
    }
}
