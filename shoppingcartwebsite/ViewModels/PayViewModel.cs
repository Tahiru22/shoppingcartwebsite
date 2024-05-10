using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.ViewModels
{
    public class PayViewModel
    {
        public string CardNumber { get; set; }
        public string CVC { get; set; }
        public Order? Order { get; set; }
    }
}
