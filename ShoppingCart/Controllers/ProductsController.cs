using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Infrastructure;
using ShoppingCart.Models;
using ShoppingCart.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
     
        //Get / products
        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 6;
            ProductsCatalogViewModel catalog = new ProductsCatalogViewModel
            {
                PageSize = p,
                PageRange = pageSize,
                TotalPages = (int)Math.Ceiling((decimal)_context.Products.Count() / pageSize),
                Products = await _context.Products.OrderByDescending(x => x.ID)
                                                   .Skip((p - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .ToListAsync()
            };
            return View(catalog);
        }
   
        // Get / products / category
        public async Task<IActionResult> ProductsByCategory(string categorySlug, int p = 1)
        {
            Category category = await _context.Categories.Where(x => x.Slug == categorySlug).FirstOrDefaultAsync();

            if (category == null)  return RedirectToAction("Index"); 

            int pageSize = 6;

            ProductsCatalogViewModel catalog = new ProductsCatalogViewModel
            {
                PageNumber = p,
                PageRange = pageSize,
                TotalPages = (int)Math.Ceiling((decimal)_context.Products.Where(w => w.CategoryId == category.ID).Count() / pageSize),
                Category = category.Name,
                Products = await _context.Products.OrderByDescending(x => x.ID)
                                                  .Where(w => w.CategoryId == category.ID)
                                                  .Skip((p - 1) * pageSize)
                                                  .Take(pageSize)
                                                  .ToListAsync()
            };
            return View(catalog);
        }
    }
}
