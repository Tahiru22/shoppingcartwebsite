namespace shoppingcartwebsite.Models
{
    public class PromoCode : BaseEntity
    {
        public string Code { get; set; }
        public int CodeDiscount { get; set; }
    }
}
