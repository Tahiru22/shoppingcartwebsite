using Microsoft.AspNetCore.Identity;

namespace shoppingcartwebsite.Models
{
    public class User : IdentityUser<Guid>
    {
        

        public string FirstName { get; set; }

        public string? SecondName { get; set; }

        public string LastName { get; set; }
    }
}
