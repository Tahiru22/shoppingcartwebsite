using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.Data
{
    public class DataUpdateService
    {
        private readonly DatabaseContext _context;
        public DataUpdateService(DatabaseContext context)
        {
            _context = context;
        }

        public void UpData()
        {
            _context.Categorys.AddRange(new Category
            {
                Name = "test",
                PathToImage = @"/img/category/category_pod.jpg",
                Products = new List<Product>()
            {
                new()
                {
                    Amount = 10,
                    Description = "test",
                    Name = "Rincoe Jellybox Nano 2 Pod Kit 26W 900mAh",
                    Price = 1990,
                    PathToImage = @"/img/products/Pod/1.jpg",
                    Discount = 20
                },
                new()
                {
                    Amount = 10,
                    Description = "Тип затяжки: Сигаретная Ёмкость аккумулятора: 750",
                    Name = "Smoant Charon Baby POD Kit 750 mah with картридж Charon Baby",
                    Price = 1890,
                    PathToImage = @"/img/products/Pod/2.jpg",
                    Discount = 10
                },
                new()
                {
                    Amount = 10,
                    Description = "test",
                    Name = "Набор Elf Bar Elfa D20 10W 850mAh",
                    Price = 650,
                    PathToImage = @"/img/products/Pod/3.jpg"
                },
                new()
                {
                    Amount = 10,
                    Description = "test",
                    Name = "Rincoe Jellybox F Pod Kit 15W 650mAh",
                    Price = 1190,
                    PathToImage = @"/img/products/Pod/4.jpg"
                },
                new()
                {
                    Amount = 10,
                    Description = "test: 1000",
                    Name = "Smoant Charon Baby Plus Pod Kit",
                    Price = 2290,
                    PathToImage = @"/img/products/Pod/5.jpg",
                    Discount = 20
                }
               
               
            }
            
               
          
            });


            _context.SaveChanges();
        }

    }
}
