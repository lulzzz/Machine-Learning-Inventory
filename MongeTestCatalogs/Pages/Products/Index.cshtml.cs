using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MongeTestCatalogs.Models;

namespace MongeTestCatalogs.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly MongeTestCatalogs.Models.ProductContext _context;

        public IndexModel(MongeTestCatalogs.Models.ProductContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get;set; }

        public async Task OnGetAsync()
        {
            Product = await _context.Product.ToListAsync();
        }
    }
}
