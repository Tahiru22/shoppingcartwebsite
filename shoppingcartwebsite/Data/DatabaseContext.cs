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
        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);
        //    SeedRoles(builder);
        //    SeedAdminUser(builder);


        //}

        //private void SeedRoles(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<IdentityRole>().HasData(
        //        new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
        //     new IdentityRole { Id = "2", Name = "Customer", NormalizedName = "Customer" }
        //    // Add other roles as needed
        //    );
        //}

        //private void SeedAdminUser(ModelBuilder modelBuilder)
        //{
        //    var hasher = new PasswordHasher<User>();

        //    modelBuilder.Entity<User>().HasData(new User
        //    {
        //        Id = "1",
        //        FirstName = "Admin",
        //        SecondName = "Admin",
        //        LastName = "Admin",
        //        Email = "admin@gmail.com",
        //        UserName = "admin@gmail.com",
        //        PhoneNumber = "0542345603",
        //        PasswordHash = hasher.HashPassword(null, "P@ssword233")
        //    });

        //    modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        //    {
        //        RoleId = "1",
        //        UserId = "1"
        //    });
        //}
    }
}
