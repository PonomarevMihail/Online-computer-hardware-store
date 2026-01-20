using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KursachBD.Models;

namespace KursachBD.Pages.Product_price_history
{
    public class EditModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public EditModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProductPriceHistory ProductPriceHistory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productpricehistory =  await _context.ProductPriceHistories.FirstOrDefaultAsync(m => m.HistoryId == id);
            if (productpricehistory == null)
            {
                return NotFound();
            }
            ProductPriceHistory = productpricehistory;
           ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Characteristics");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ProductPriceHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductPriceHistoryExists(ProductPriceHistory.HistoryId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProductPriceHistoryExists(long id)
        {
            return _context.ProductPriceHistories.Any(e => e.HistoryId == id);
        }
    }
}
