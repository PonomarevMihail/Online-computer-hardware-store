using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using KursachBD.Models;

namespace KursachBD.Pages.Product_price_history
{
    public class CreateModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public CreateModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Characteristics");
            return Page();
        }

        [BindProperty]
        public ProductPriceHistory ProductPriceHistory { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ProductPriceHistories.Add(ProductPriceHistory);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
