using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.ViewModel
{
    public class ProductsCatalogViewModel
    {
        public List<Product> Products { get; set; }

        public int PageRange { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public string Category { get; set; }
    }
}
