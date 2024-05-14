using System.ComponentModel.DataAnnotations.Schema;

namespace shoppingcartwebsite.Models
{

    public class Order : BaseEntity
    {
        public OrderStatus OrderStatus { get; set; }

        public ICollection<ProductBasket> Products { get; set; }

        public DateTime DateTime { get; set; }
        public int? Quantity { get; set; }
        public Double Price { get; set; }

        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public Client? Client { get; set; }
    }
}
