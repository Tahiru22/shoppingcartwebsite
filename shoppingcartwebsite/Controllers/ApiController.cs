using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.Controllers
{
    public class ApiController : Controller
    {
        private readonly DatabaseContext _context;

        public ApiController(DatabaseContext context)
        {
            _context = context;
        }


        [Route("Category/get")]
        public async Task<IEnumerable<Category>> GetCategorys()
            => await _context.Categorys
                .Include(x => x.Products)
                .ToArrayAsync();

        //[Route("Client/get")]
        public async Task<IEnumerable<Client>> GetClients()
            => await _context.Clients
                .Include(x => x.Basket)
                .ThenInclude(x => x.Product)
                .Include(x => x.Orders)
                .ThenInclude(x => x.Products)
                .ThenInclude(x => x.Product)
                .Include(x => x.FavoriteProducts)
                .ToArrayAsync();

        [Route("Order/get")]
        public async Task<IEnumerable<Order>> GetOrders()
            => await _context.Orders
                .Include(x => x.Products)
                .ThenInclude(x => x.Product)
                .ToArrayAsync();

        [Route("Product/get")]
        public async Task<IEnumerable<Product>> GetProduct()
            => await _context.Products
                .ToArrayAsync();

        [Route("ProductBasket/get")]
        public async Task<IEnumerable<ProductBasket>> GetProductBaskets()
            => await _context.ProductBaskets
                .Include(x => x.Product)
                .ToArrayAsync();

        [Route("User/get")]
        public async Task<IEnumerable<User>> GetUsers()
            => await _context.Users
                .ToArrayAsync();
    }
}
