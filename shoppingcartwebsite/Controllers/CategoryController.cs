using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shoppingcartwebsite.Data;
using shoppingcartwebsite.Models;

namespace shoppingcartwebsite.Controllers
{
     [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        public CategoryController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env, DatabaseContext context)
        {
            _env = env;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categorys.ToListAsync();
            return View(categories);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Save the image file
                string imagePath = await SaveImage(imageFile);

                var category = new Category
                {
                    Name = model.Name,
                    PathToImage = imagePath
                };

                _context.Categorys.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categorys.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new Category
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Category model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categorys.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                // Update category properties
                category.Name = model.Name;

                // Check if a new image file was uploaded
                if (imageFile != null)
                {
                    // Delete old image file
                    DeleteImage(category.PathToImage);
                    // Save the new image file
                    category.PathToImage = await SaveImage(imageFile);
                }

                _context.Categorys.Update(category);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        //[HttpPost]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var category = await _context.Categorys.FindAsync(id);
        //    if (category == null)
        //    {
        //        return NotFound();
        //    }

        //    // Delete image file
        //    DeleteImage(category.PathToImage);

        //    _context.Categorys.Remove(category);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("Index");
        //}


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categorys.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if there are associated products
            var associatedProducts = await _context.Products.Where(p => p.Id == id).ToListAsync();
            if (associatedProducts.Any())
            {
                // Delete images associated with products
                foreach (var product in associatedProducts)
                {
                    if (!string.IsNullOrEmpty(product.PathToImage))
                    {
                        DeleteImage(product.PathToImage);
                    }
                }

                // Remove associated products
                _context.Products.RemoveRange(associatedProducts);
            }

            // Delete category image
            if (!string.IsNullOrEmpty(category.PathToImage))
            {
                DeleteImage(category.PathToImage);
            }

            // Remove category
            _context.Categorys.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return null;
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "img");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/img/" + uniqueFileName; // Return the relative path to the saved image
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
