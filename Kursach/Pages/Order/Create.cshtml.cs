using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using KursachBD.Models;

namespace KursachBD.Pages.Order
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
            return Page();
        }

        [BindProperty]
        public KursachBD.Models.Order Order { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Order.PaymentDate == null)
            {
                Order.PaymentDate = DateTime.Now;
            }



            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
