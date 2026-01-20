using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KursachBD.Models;

namespace KursachBD.Pages.Legal_entity
{
    public class DetailsModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public DetailsModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public LegalEntity LegalEntity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var legalentity = await _context.LegalEntities.FirstOrDefaultAsync(m => m.Inn == id);

            if (legalentity is not null)
            {
                LegalEntity = legalentity;

                return Page();
            }

            return NotFound();
        }
    }
}
