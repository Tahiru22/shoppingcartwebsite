using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace shoppingcartwebsite.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Amount { get; set; }
        public double Price { get; set; }

        public int? Discount { get; set; }
        public string? PathToImage { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
		public int NumberSales { get; set; }
        [DisplayName("Category")]
        public int CategoryId { get; set; } 
        public Category? Category { get; set; }
    }
}
