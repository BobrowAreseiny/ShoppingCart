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
    [Area("Admin")]
    public class PagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET /admin/pages
        public async Task<IActionResult> Index()
        {
            IQueryable<Page> pages = from p in _context.Pages orderby p.Sorting select p;

            List<Page> pagesList = await pages.ToListAsync();

            return View(pagesList);// Добавить название метода в первый аргумент, если его поменяли.
        }

        //GET /admin/pages/details
        public async Task<IActionResult> Details(int id)
        {
            Page page = await _context.Pages.FirstOrDefaultAsync(f => f.ID == id);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        //GET /admin/pages/create
        public IActionResult Create() => View();

        //POST /admin/pages/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Page page)
        {
            if (ModelState.IsValid)
            {
                page.Slug = page.Title.ToLower().Replace(" ","-");
                page.Sorting = 100;

                var slug = await _context.Pages.FirstOrDefaultAsync(x => x.Slug == page.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("","This page already exists.");

                    return View(page);
                }

                _context.Add(page);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This page has been added!";

                return RedirectToAction("Index");
            }
            return View(page);
        }

        //GET /admin/pages/edit
        public async Task<IActionResult> Edit(int id)
        {
            Page page = await _context.Pages.FirstOrDefaultAsync(f => f.ID == id);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        //POST /admin/pages/edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Page page)
        {
            if (ModelState.IsValid)
            {
                page.Slug = page.ID == 1 ? "home" : page.Title.ToLower().Replace(" ", "-");               

                var slug = await _context.Pages.Where(x => x.ID != page.ID).FirstOrDefaultAsync(x => x.Slug == page.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This page already exists.");

                    return View(page);
                }

                _context.Update(page);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This page has been edited!";

                return RedirectToAction("Edit", new { id = page.ID });
            }
            return View(page);
        }

        //GET /admin/pages/delete
        public async Task<IActionResult> Delete(int id)
        {
            Page page = await _context.Pages.FirstOrDefaultAsync(f => f.ID == id);

            if (page == null)
            {
                TempData["Error"] = "This page does not exist!";
            }
            else
            {
                _context.Pages.Remove(page);
                await _context.SaveChangesAsync();

                TempData["Success"] = "This page has been deleted!";
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
                Page page = await _context.Pages.FindAsync(pageId);
                page.Sorting = count;
                _context.Update(page);
                await _context.SaveChangesAsync();
                count++;
            }

            return Ok();
        }
    }
}
