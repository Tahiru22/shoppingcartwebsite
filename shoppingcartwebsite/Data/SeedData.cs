using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(DatabaseContext _db, UserManager<User> _userManager, RoleManager<ApplicationRole> _roleManager)
        {
            _db.Database.Migrate();
            // Seed roles
            if (!_db.Roles.Any())
            {
                await _roleManager.CreateAsync(new ApplicationRole(Roles.Admin));
                await _roleManager.CreateAsync(new ApplicationRole(Roles.Customer));
            
            }

            if (!_db.Users.Any())
            {
                var admin = new User()
                {
                    FirstName = "Admin",
                    SecondName = "Admin",
                    LastName = "Admin",
                    Email = "admin@gmail.com",
                    UserName = "admin@gmail.com",
                    PhoneNumber = "0542345603"

                };
                await _userManager.CreateAsync(admin, "Admin@123");
                await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }


    }

}
