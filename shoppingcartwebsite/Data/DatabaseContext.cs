using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.Data
{
    public class DatabaseContext : IdentityDbContext<User, ApplicationRole, Guid>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
          : base(options)
        {
        }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ShopPoint> ShopPoints { get; set; }

        public DbSet<ProductBasket> ProductBaskets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>()
    .HasOne(o => o.Client)
    .WithMany(c => c.Orders)
    .HasForeignKey(o => o.ClientId)
    .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
