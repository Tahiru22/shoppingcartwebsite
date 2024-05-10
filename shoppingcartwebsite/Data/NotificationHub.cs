using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace shoppingcartwebsite.Data
{
 

    public class NotificationHub : Hub
    {
        public static string LatestOrderUser { get; private set; } = null;

        public async Task NewOrderCreated(string userName)
        {
            LatestOrderUser = userName;
            await Clients.All.SendAsync("NewOrderNotification", userName);
        }
    }

}
