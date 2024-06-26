﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.Service;
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
        private readonly EmailSender _emailSender;
        private readonly IEmailPasswordSender _emailPasswordSender;
        public AccountController(DatabaseContext context, IEmailPasswordSender emailPasswordSender, EmailSender emailSender, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _emailPasswordSender = emailPasswordSender;
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
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber

                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
					// Add the user to the "Admin" role
					await _userManager.AddToRoleAsync(user, "Customer");

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

//=============================================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);
            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Reset password token", callback, null);
            await _emailPasswordSender.SendEmailAsync(message);
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return View();
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        //=====================================

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
                            
                            return RedirectToAction("Diagrams", "Admin"); 
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
