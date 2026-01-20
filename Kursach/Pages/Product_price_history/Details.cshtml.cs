using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KursachBD.Models;

namespace KursachBD.Pages.Product_price_history
{
    public class DetailsModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public DetailsModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public ProductPriceHistory ProductPriceHistory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productpricehistory = await _context.ProductPriceHistories.FirstOrDefaultAsync(m => m.HistoryId == id);

            if (productpricehistory is not null)
            {
                ProductPriceHistory = productpricehistory;

                return Page();
            }

            return NotFound();
        }
    }
}
