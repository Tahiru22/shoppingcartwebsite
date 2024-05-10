using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.ViewModels;
using Microsoft.AspNetCore.SignalR;
using shoppingcartwebsite.Service;

namespace shoppingcartwebsite.Controllers
{
    public class OrderController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        //private readonly IEmailService _emailService;
        private readonly EmailSender _emailSender;
        public OrderController(DatabaseContext context, EmailSender emailSender, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
          
            _emailSender = emailSender;
        }

        [Authorize]
        public async Task<IActionResult> PayOrder()
        {
            
            return View();
        }


        [Authorize]
        public async Task<RedirectToActionResult> CreateOrder(PayViewModel model)
        {

            if (model.CardNumber.Length != 19)
                return RedirectToAction("Basket", "Basket");

            if (model.CVC.Length != 3)
                return RedirectToAction("Basket", "Basket");


            var email = User.Identity?.Name;
            var client = await _context.Clients
                .Include(x => x.User)
                .Include(x => x.Orders)
                .ThenInclude(x => x.Products)
                .Include(x => x.Basket)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.User.Email == email);

            if (client.Basket.Count > 0)
            {
               
                var sumWithDiscount = client.Basket
     .Where(x => x.Product?.Discount > 0)
     .Select(x => x.Amount * x.Product?.Price * (100 - x.Product?.Discount) / 100)
     .Sum();

                var sumWithoutDiscount = client.Basket
                   .Where(x => x.Product?.Discount == 0)
                   .Select(x => x.Amount * x.Product?.Price)
                   .Sum();

                var order = new Order
                {
                    Products = new List<ProductBasket>(),
                    DateTime = DateTime.Now,
                    OrderStatus = OrderStatus.Collecting,
                    Price  = sumWithDiscount.GetValueOrDefault(0.0) + sumWithoutDiscount.GetValueOrDefault(0.0)

                    //   Price = sumWithDiscount.HasValue ? (sumWithDiscount.Value == 0 ? sumWithoutDiscount.GetValueOrDefault(0.0) : sumWithDiscount.Value + sumWithoutDiscount.GetValueOrDefault(0.0)) : sumWithoutDiscount.GetValueOrDefault(0.0)
                };

                foreach (var productBasket in client.Basket)
                {
                    order.Products.Add(productBasket);

                    productBasket.Product!.NumberSales += productBasket.Amount;
                    _context.Products.Update(productBasket.Product);
                }

              

                client.Orders.Add(order);
                client.Basket.Clear();
                _context.Clients.Update(client);
                await _context.SaveChangesAsync();

                var adminEmail = "tahirukofitey@gmail.com"; // Replace with actual admin email
                var adminSubject = "New Order Notification";
                //var adminBody = $"A new order has been created by {client.User.FirstName} {client.User.LastName} with Order ID:{order.Id}.";
                var adminBody = $@"
    <div style='font-family: Arial, sans-serif; background-color: #f8f9fa; padding: 20px;'>
        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 5px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
            <div style='padding: 30px;'>
                <h2 style='margin-top: 0; color: #333333;'>New Order Created</h2>
                <p style='margin-bottom: 20px; color: #666666; font-size: 16px;'>A new order has been created by <strong>{client.User.FirstName} {client.User.LastName}</strong> with Order ID: <strong>{order.Id}</strong> at  <strong>{order.DateTime}</strong>.</p>
       
            </div>
        </div>
    </div>
";

                _emailSender.SendEmail(adminEmail, adminSubject, adminBody);

            }

            return RedirectToAction("Orders", "Account");
        }
    }
}
