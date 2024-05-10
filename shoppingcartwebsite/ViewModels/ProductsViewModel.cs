using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.ViewModels
{
    public class ProductsViewModel
    {
        public string Title { get; set; }
        public ICollection<Product> Products { get; set; }

		public ICollection<Category> Categories { get; set; }

		public Client? Client { get; set; }
    }
}
