using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Infrastructure;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET /admin/categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.OrderBy(x => x.Sorting).ToListAsync());
        }

        //GET /admin/categories/create
        public IActionResult Create() => View();

        //POST /admin/categories/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");
                category.Sorting = 100;

                var slug = await _context.Categories.FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This category already exists.");

                    return View(category);
                }

                _context.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This category has been added!";

                return RedirectToAction("Index");
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            Category category = await _context.Categories.FirstOrDefaultAsync(f => f.ID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        //POST /admin/categories/edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.ToLower().Replace(" ", "-");

                var slug = await _context.Categories.Where(x => x.ID != id).FirstOrDefaultAsync(x => x.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This category already exists.");

                    return View(category);
                }

                _context.Update(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This category has been edited!";

                return RedirectToAction("Edit", new { id });
            }
            return View(category);
        }

        //GET /admin/categories/delete
        public async Task<IActionResult> Delete(int id)
        {
            Category category = await _context.Categories.FirstOrDefaultAsync(f => f.ID == id);

            if (category == null)
            {
                TempData["Error"] = "This category does not exist!";
            }
            else
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This category has been deleted!";
            }

            return RedirectToAction("Index");
        }

        //POST /admin/pages/reorder
        [HttpPost]
        public async Task<IActionResult> Reorder(int[] id)
        {
            int count = 1;

            foreach (var pageId in id)
            {
                Category category = await _context.Categories.FindAsync(pageId);
                category.Sorting = count;
                _context.Update(category);
                await _context.SaveChangesAsync();
                count++;
            }

            return Ok();
        }
    }
}
