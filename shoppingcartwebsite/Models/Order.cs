namespace shoppingcartwebsite.Models
{

    public class Order : BaseEntity
    {
        public OrderStatus OrderStatus { get; set; }

        public ICollection<ProductBasket> Products { get; set; }

        public DateTime DateTime { get; set; }

        public Double Price { get; set; }
    }
}
