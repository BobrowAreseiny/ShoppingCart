using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Infrastructure
{
    public class MainMenuViewComponent : ViewComponent 
    {
        public readonly ApplicationDbContext _context;

        public MainMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pages = await GetPagesAsync();
            return View(pages);
        }

        private Task<List<Page>> GetPagesAsync()
        {
            return _context.Pages.OrderBy(x => x.Sorting).ToListAsync();
        }
    }
}
