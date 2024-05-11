using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;
using shoppingcartwebsite.ViewModels;

namespace shoppingcartwebsite.Controllers
{
   
    public class ProductController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        public ProductController(DatabaseContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {

            _context = context;
            _env = env;
            
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).OrderByDescending(p => p.DateTime).ToListAsync();
            return View(products);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var categories = _context.Categorys
                                        .Select(c => new SelectListItem
                                        {
                                            Value = c.Id.ToString(),
                                            Text = c.Name
                                        }).ToList();

            ViewBag.Categories = new SelectList(categories, "Value", "Text");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Product product, [Bind("CategoryId")] IFormFile image)
        {
            if (ModelState.IsValid)
            {
                // Save the image
                var imagePath = await SaveImageAsync(image);
                product.PathToImage = imagePath;

               
                if (product.CategoryId == 0)
                {
                    ModelState.AddModelError("CategoryId", "The Category field is required.");
                    return View(product);
                }


                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
               
            }

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    // Log or handle the error as needed
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(product);
        }


		

		public async Task<IActionResult> Details(int id)
		{

			var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
			if (product == null)
			{
				return NotFound(); // Product not found
			}

			var categories = await _context.Categorys.ToListAsync();
			var viewModel = new IndexViewModel
			{
				Categories = categories,
				NewProducts = new List<Product> { product }, // Assuming you want to display only this product as a new product
				Client = null // Set client information as needed
			};

			return View(viewModel);
		}

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            // Fetch the product to be edited from the database
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(); // Product not found
            }

            
            var categories = await _context.Categorys.ToListAsync();

           
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Update product properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Amount = product.Amount;
                existingProduct.Price = product.Price;
                existingProduct.Discount = product.Discount;
                existingProduct.NumberSales = product.NumberSales;
                existingProduct.CategoryId = product.CategoryId; // Assuming CategoryId is the foreign key property

                // Check if a new image file was uploaded
                if (imageFile != null)
                {
                    // Delete old image file
                    DeleteImage(existingProduct.PathToImage);
                    // Save the new image file
                    existingProduct.PathToImage = await SaveImageAsync(imageFile);
                }

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }


            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    // Log or handle the error as needed
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            // If model state is not valid, return the form with errors
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Delete the image file
            DeleteImage(product.PathToImage);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Path.Combine("/img", uniqueFileName).Replace("\\", "/");
        }
        private void DeleteImage(string imagePath)
        {
            if (imagePath != null)
            {
                string filePath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
