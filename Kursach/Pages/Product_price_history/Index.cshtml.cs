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
    public class IndexModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public IndexModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public IList<ProductPriceHistory> ProductPriceHistory { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ProductPriceHistory = await _context.ProductPriceHistories
                .Include(p => p.Product).ToListAsync();
        }
    }
}
