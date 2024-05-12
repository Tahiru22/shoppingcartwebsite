using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.ViewModels;
using System.Diagnostics;

namespace shoppingcartwebsite.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseContext _context;

        public HomeController(ILogger<HomeController> logger, DatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

       

        public async Task<IActionResult> Index()
        {
            var email = User.Identity?.Name;
            Client? client = null;
            if (email != null)
            {
                client = await _context.Clients
                    .Include(x => x.User)
                    .Include(x => x.Orders)
                    .Include(x => x.FavoriteProducts)
                    .FirstOrDefaultAsync(x => x.User.Email == email);
            }

            var categories = await _context.Categorys.OrderByDescending(p => p.DateTime).ToListAsync();
            //var products = await _context.Products.ToListAsync();
            var products = await _context.Products.OrderByDescending(p => p.DateTime).ToListAsync();

            var model = new IndexViewModel
            {
                Categories = categories,
                NewProducts = products,
                Client = client
            };

            return View(model);
        }

        public async Task<IActionResult> AllProducts()
        {
            var email = User.Identity?.Name;
            Client? client = null;
            if (email != null)
            {
                client = await _context.Clients
                    .Include(x => x.User)
                    .Include(x => x.Orders)
                    .Include(x => x.FavoriteProducts)
                    .FirstOrDefaultAsync(x => x.User.Email == email);
            }

            var products = await _context.Products.OrderByDescending(p => p.DateTime).ToListAsync();
			var categories = await _context.Categorys.OrderByDescending(p => p.DateTime).ToListAsync();
			var model = new ProductsViewModel
            {
                Title = "All Products",
                Client = client,
                Products = products,
                Categories = categories
			};

            return View(model);
        }


        public async Task<IActionResult> Products(int idCategory)
        {
            var category = _context.Categorys
                .Include(x => x.Products)
                .FirstOrDefault(x => x.Id == idCategory);

            var list = category?.Products;
            var title = category?.Name;

            var email = User.Identity?.Name;
            Client? client = null;
            if (email != null)
            {
                client = await _context.Clients
                    .Include(x => x.User)
                    .Include(x => x.FavoriteProducts)
                    .FirstOrDefaultAsync(x => x.User.Email == email);
            }
            var model = new ProductsViewModel
            {
                Title = title ?? "No catgory",
                Client = client,
                Products = list ?? new List<Product>()
            };

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> Products(string? search)
        {
            var list = await _context.Products
                .Where(x => x.Name.ToLower().Contains(search == null ? "" : search.ToLower()))
                .ToListAsync();

            var email = User.Identity?.Name;
            Client? client = null;
            if (email != null)
            {
                client = await _context.Clients
                    .Include(x => x.User)
                    .Include(x => x.FavoriteProducts)
                    .FirstOrDefaultAsync(x => x.User.Email == email);
            }

            var model = new ProductsViewModel
            {
                Title = "Your search results",
                Client = client,
                Products = list
            };

            return View(model);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}