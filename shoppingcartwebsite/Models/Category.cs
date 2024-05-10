namespace shoppingcartwebsite.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public string? PathToImage { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
		public ICollection<Product>? Products { get; set; }
    }
}
