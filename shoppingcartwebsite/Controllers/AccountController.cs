using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.ViewModels;
using System.Security.Claims;

namespace shoppingcartwebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _context;
        //private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AccountController(DatabaseContext context, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
          
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Authorization() => View();
        public IActionResult Registration() => View();



        [Authorize]
        public async Task<IActionResult> AccountClient()
        {
            var email = User.Identity?.Name;

            var client = await _context.Clients
                .Include(x => x.User)
                .Include(x => x.FavoriteProducts)
                .Include(x => x.Orders)
                .ThenInclude(x => x.Products)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.User.Email == email);

            return View(client);
        }

        [Authorize]
        public async Task<IActionResult> Orders()
        {
            var email = User.Identity?.Name;

            var client = await _context.Clients
                .Include(x => x.User)
                .Include(x => x.Orders)
                .ThenInclude(x => x.Products)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.User.Email == email);
            // Sort orders by DateTime in descending order
            client.Orders = client.Orders.OrderByDescending(o => o.DateTime).ToList();

            return View(client);
        }


        [Authorize]
        public async Task<RedirectToActionResult> UpdateFavoriteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product != null)
            {
                var email = User.Identity?.Name;
                var client = await _context.Clients
                    .Include(x => x.FavoriteProducts)
                    .FirstOrDefaultAsync(x => x.User.Email == email);

                if (client != null)
                {
                    var _ = client.FavoriteProducts.FirstOrDefault(x => x.Id == id);
                    if (_ == null)
                        client.FavoriteProducts.Add(product);
                    else
                        client.FavoriteProducts.Remove(product);

                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("AccountClient", "Account");
        }


        

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var findUser = await _userManager.FindByEmailAsync(model.Email);

            if (findUser != null) return View(model);

            if (model.Password.Length > 7)
            {
                var user = new User
                {
                    UserName = model.Email, // Using email as username
                    Email = model.Email,
                    FirstName = model.FirstName,
                    SecondName = model.SecondName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber

                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await Authenticate(user.Email);

                    var client = new Client
                    {
                        User = user,
                        Balance = 0,
                        FavoriteProducts = new List<Product>(),
                        Basket = new List<ProductBasket>(),
                        Orders = new List<Order>()
                    };

                    await _context.Clients.AddAsync(client);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("AccountClient", "Account");
                }
                AddErrors(result);
            }
            ModelState.AddModelError("", "An error occurred");
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }



        [HttpPost]
        public async Task<IActionResult> Authorization(AuthorizationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                    if (result.Succeeded)
                    {
                        // Check if the user is an admin
                        bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                        if (isAdmin)
                        {
                            
                            return RedirectToAction("EasyData", "Admin"); 
                        }
                        else
                        {
                            
                            await Authenticate(user.Email);
                            return RedirectToAction("AccountClient", "Account");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred");
                }
            }
            return View(model);
        }


        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new (ClaimsIdentity.DefaultNameClaimType, userName)
            };

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Authorization", "Account");
        }
    }
}
