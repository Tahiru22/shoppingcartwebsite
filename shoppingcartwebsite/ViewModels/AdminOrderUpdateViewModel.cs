using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.ViewModels
{
    public class AdminOrderUpdateViewModel
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}
