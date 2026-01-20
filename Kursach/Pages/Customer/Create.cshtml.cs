using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using KursachBD.Models;
using BCrypt.Net;
namespace KursachBD.Pages.Customer
{
    public class CreateModel : PageModel
    {
        private readonly KursachBD.Models.OnlineStoreSellingComputerEquipmentContext _context;

        public CreateModel(KursachBD.Models.OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(long? id)
        {
         
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "JobTitle");
            return Page();
        }


        [BindProperty]
        public KursachBD.Models.Customer Customer { get; set; } = default!;

        
        public async Task<IActionResult> OnPostAsync()
{
        
        if (!ModelState.IsValid)
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "JobTitle");
            return Page();
        }

        if (_context.Customers.Any(c => c.Login == Customer.Login))
        {
           ModelState.AddModelError("Customer.Login", "Пользователь с таким логином уже существует.");
           ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "JobTitle");
           return Page();
        }

        if (_context.Customers.Any(c => c.Email == Customer.Email && c.CustomerId != Customer.CustomerId))
        {
          ModelState.AddModelError("Customer.Email", "Пользователь с таким email уже зарегистрирован.");
          ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "JobTitle");
          return Page();
        }

        Customer.Password = BCrypt.Net.BCrypt.HashPassword(Customer.Password);

            _context.Customers.Add(Customer);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
}
    }
}
