using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace shoppingcartwebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly string _set = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "set " : "export ";
        private readonly UserManager<User> _userManager;
        public AdminController(DatabaseContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult EasyData() => View();
        public async Task<IActionResult> Diagrams() => View(await GetDataForCharts());



        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            return View(users);
        }

       

        [HttpPost]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var unlockResult = await _userManager.SetLockoutEnabledAsync(user, false);
                if (unlockResult.Succeeded)
                {
                    // Optionally, clear the lockout end date
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.MinValue);
                    return RedirectToAction("GetAllUsers");
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); // User not found
            }

            //user.IsBlocked = true; // Assuming you have a property like IsBlocked in your User entity
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // User blocked successfully
                return RedirectToAction("Index"); // Redirect to a suitable page
            }
            else
            {
                // Handle errors
                return View("Error"); // Show error view
            }
        }

        public IActionResult DeleteUser() => View();

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(); // User not found
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                // User deleted successfully
                return RedirectToAction("GetAllUsers"); // Redirect to a suitable page
            }
            else
            {
                // Handle errors
                return View("Error"); // Show error view
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> DeleteUser(string userId)
        //{
        //    var userGuid = new Guid(userId);
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    // Fetch the Client associated with the User
        //    var client = await _context.Clients
        //        .Include(c => c.Orders)
        //            .ThenInclude(o => o.Products)
        //        .FirstOrDefaultAsync(c => c.User.Id == userGuid);

        //    if (client != null)
        //    {
        //        // Handle related Orders and Products
        //        foreach (var order in client.Orders)
        //        {
        //            _context.Orders.Remove(order);
        //        }

        //        // After handling related entities, delete the Client
        //        _context.Clients.Remove(client);
        //    }

        //    // Delete the User
        //    var result = await _userManager.DeleteAsync(user);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToAction("GetAllUsers");
        //    }
        //    else
        //    {

        //        return BadRequest(result.Errors);
        //    }
        //}





        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Clients
                .Include(x => x.User)
                .Include(x => x.Orders)
                    .ThenInclude(x => x.Products)
                        .ThenInclude(x => x.Product)
                .ToListAsync();

            // Sort each client's orders by their creation date in descending order
            foreach (var client in clients)
            {
                client.Orders = client.Orders.OrderByDescending(o => o.DateTime).ToList();
            }

            // Sort clients by the latest order date
            clients = clients.OrderByDescending(x => x.Orders.Max(o => o.DateTime)).ToList();

            return View(clients);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(AdminOrderUpdateViewModel model)
        {
            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = model.NewStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetClients)); 
        }









        public async Task<ChartViewModel> GetDataForCharts()
        {
            var model = new ChartViewModel();

            
            var barLabels = await _context.Products.Select(x => x.Name).ToListAsync();
            var barPrices = await _context.Products.Select(x => x.NumberSales).ToListAsync();

            
            var records = await _context.Orders
                .Include(x => x.Products)
                .ThenInclude(x => x.Product)
                .ToListAsync();

            var now = DateTime.Now.Date.AddDays(1);
            var date = DateTime.Now.Date.AddDays(-6);

            var lineData = new List<int>();
            var lineLabels = new List<string>();

            while (now != date)
            {
                
                int count = records
                    .Where(x => x.DateTime.Date == date)
                    .Select(x => x.Products.Select(z => z.Amount).Sum())
                    .Sum();

                lineLabels.Add(date.DayOfWeek.ToString());
                lineData.Add(count);

                date = date.AddDays(1);
            }

            model.BarLabels = barLabels;
            model.BarData = barPrices;

            model.LineData = lineData;
            model.LineLabels = lineLabels;
            return model;
        }


        public async Task ExportData()
        {
            const string dirName = @"C:\tmp\csv";
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(@"C:\tmp\csv");
            await _context.Database.ExecuteSqlRawAsync("call BackupCSV();");
        }

        public ViewResult Backup()
        {
            var list = new List<string>();
            const string dirName = @"C:\tmp";
            if (Directory.Exists(dirName))
            {
                var files = Directory.GetFiles(dirName);
                list.AddRange(files.Select(Path.GetFileName)!);
            }
            else
                Directory.CreateDirectory(@"C:\tmp");

            return View(list);
        }

        public async Task Dump()
        {
            var date = DateTime.Now;
            var fileName = $"{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}";
            await PSqlDump(
                @"C:\Program Files\PostgreSQL\14\bin\",
                "333",
                "postgres",
                "VapeShop",
                $"/tmp/{fileName}");
        }

        public async Task Restore(string name)
        {
            await PSqlRestore(
                @"C:\Program Files\PostgreSQL\14\bin\",
                "333",
                "postgres",
                "VapeShop",
                $"/tmp/{name}");
        }

        private async Task PSqlDump(string pathToExecutableFile, string password, string login, string database, string outputFile)
        {
            string[] commands =
            {
            $"cd {pathToExecutableFile}",
            $"{_set} PGPASSWORD={password}",
            $"pg_dump.exe -U {login} {database} > {outputFile}.sql"
        };
            await RunCommands(commands);
        }

        private async Task PSqlRestore(string pathToExecutableFile, string password, string login, string database, string inputFile)
        {
            string[] commands =
            {
            $"cd {pathToExecutableFile}",
            $"{_set} PGPASSWORD={password}",
            $"psql -U {login} -d {database} -c \"select pg_terminate_backend(pid) from pg_stat_activity where datname = '{database}'",
            $"dropdb -U {login} {database}",
            $"createdb -U {login} {database}",
            $"psql -U {login} -d {database} -f {inputFile}",
        };
            await RunCommands(commands);
        }

        private static async Task RunCommands(string[] commands)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false

                }
            };
            process.Start();
            await using var pWriter = process.StandardInput;
            if (pWriter.BaseStream.CanWrite)
            {
                foreach (var line in commands)
                    await pWriter.WriteLineAsync(line);
            }
        }

        
    }
}
