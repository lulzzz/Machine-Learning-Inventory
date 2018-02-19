using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MongeTestCatalogs.Models;

namespace MongeTestCatalogs.Pages.VoiceCatalog
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Message { get; set; }

        [BindProperty]
        public string Type { get; set; }
        [BindProperty]
        public string Brand { get; set; }

        [BindProperty]
        public string Err { get; set; }

        private readonly ProductContext _context;

        public IndexModel(ProductContext context)
        {
            _context = context;
        }

        public IList<Product> Product { get; set; }

        public async Task OnGetAsync()
        {
            Title = "Voice Recognition Catalog";
            Message = "Click on the Recognition Button to create a new request.";

            Product = await _context.Product.ToListAsync();
        }

        public IActionResult OnPost()
        {
            
            Message = "Message Changed";
            //return Page();
            return new JsonResult("Post --> Get Product");
        }


        [ValidateAntiForgeryToken]
        public IActionResult GetProducts()
        {
           Message = "Message Changed";
           //return new JsonResult("Get  Product");
           return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnGetVoiceCatalog( string type)
        {
            var brand = "";
            Title = "Voice Recognition Catalog";
            Message = "You have search products with the following characteristics:";
            Type = type;
            Brand = brand;

            var query = from p in _context.Product select p;

            if (!String.IsNullOrEmpty(type))
            {
                query = query.Where(s => s.Title.Contains(type));
            }
            Product = await query.ToListAsync();

            return Page();
        }

    }
}