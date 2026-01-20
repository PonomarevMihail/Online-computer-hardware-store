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
    public class IndexModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public IndexModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public IList<LegalEntity> LegalEntity { get;set; } = default!;

        public async Task OnGetAsync()
        {
            LegalEntity = await _context.LegalEntities.ToListAsync();
        }
    }
}
