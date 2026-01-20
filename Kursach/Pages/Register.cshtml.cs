using KursachBD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace KursachBD.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;

        public RegisterModel(OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

       
        public class RegisterInputModel : KursachBD.Models.Customer
        {
            [Required(ErrorMessage = "Подтвердите пароль")]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; } = "";
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Проверка на уникальность логина
            if (await _context.Customers.AnyAsync(c => c.Login == Input.Login))
            {
                ModelState.AddModelError("Input.Login", "Такой логин уже занят");
                return Page();
            }

            // Проверка на уникальность Email
            if (await _context.Customers.AnyAsync(c => c.Email == Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Такой Email уже зарегистрирован");
                return Page();
            }

            //Хеширование пароля
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password);

            
            var newCustomer = new KursachBD.Models.Customer
            {
                Name = Input.Name,
                Surname = Input.Surname,
                Patronymic = Input.Patronymic,
                Login = Input.Login,
                Email = Input.Email,
                Phone = Input.Phone,
                Password = passwordHash, 
                RoleId = 4 
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            
            return RedirectToPage("/Authoriz", new { registered = true });
        }
    }
}