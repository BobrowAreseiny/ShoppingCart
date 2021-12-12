using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Infrastructure;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 6;
            var products = await _context.Products
                .OrderByDescending(x => x.ID)
                .Include(x => x.Category).Skip((p-1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Products.Count() / pageSize);
            return View(products);
        }

        //GET /admin/products/create
        public IActionResult Create() 
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.OrderBy(x => x.Sorting),"ID", "Name");
            return View(); 
        }

        //POST /admin/products/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.OrderBy(x => x.Sorting), "ID", "Name");
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-");

                var slug = await _context.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This product already exists.");
                    return View(product);
                }

                string imageName = "noimage.png";

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;

                }

                _context.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This product has been added!";

                return RedirectToAction("Index");
            }
            return View(product);
        }

        //GET /admin/products/details
        public async Task<IActionResult> Details(int id)
        {
            Product product = await _context.Products.Include(x => x.Category).FirstOrDefaultAsync(f => f.ID == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //GET /admin/products/edit
        public async Task<IActionResult> Edit(int id)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(f => f.ID == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.CategoryId = new SelectList(_context.Categories.OrderBy(x => x.Sorting), "ID", "Name",product.CategoryId);
            return View(product);
        }

        //POST /admin/products/edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.OrderBy(x => x.Sorting), "ID", "Name", product.CategoryId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-");

                var slug = await _context.Products.Where(x => x.ID != id).FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This page already exists.");

                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media\\products");

                    if(!string.Equals(product.Image, "noimage.png"))
                    {
                        string oldImagePath = Path.Combine(uploadsDir, product.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;

                }

                _context.Update(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This product has been edited!";

                return RedirectToAction("Index");
            }
            return View(product);
        }

        //GET /admin/pages/delete
        public async Task<IActionResult> Delete(int id)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(f => f.ID == id);

            if (product == null)
            {
                TempData["Error"] = "This product does not exist!";
            }
            else
            {
                if (!string.Equals(product.Image, "noimage.png"))
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string oldImagePath = Path.Combine(uploadsDir, product.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This product has been deleted!";
            }
            return RedirectToAction("Index");
        }

    }
}
