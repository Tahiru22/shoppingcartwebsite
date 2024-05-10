using Microsoft.AspNetCore.Identity;

namespace shoppingcartwebsite.Models
{
    public class ApplicationRole: IdentityRole<Guid>
    {
        public ApplicationRole() : base()
        {

        }
        public ApplicationRole(string roleName) : base(roleName)
        {

        }
    }
}
