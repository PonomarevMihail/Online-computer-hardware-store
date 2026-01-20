using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using KursachBD.Models;
using BCrypt.Net;

namespace KursachBD.Pages
{
    public class AuthorizModel : PageModel
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;

        public AuthorizModel(OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Login { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public void OnGet(bool registered = false)
        {
            if (registered)
            {
                ViewData["SuccessMessage"] = "Регистрация успешна! Теперь вы можете войти.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                return Page();
            }

            var user = await _context.Customers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                ErrorMessage = "Неверный логин или пароль";
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.CustomerId.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim("FullName", $"{user.Name} {user.Surname}"),
                new Claim(ClaimTypes.Role, user.Role.JobTitle)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToPage("/Index");
        }

        
        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");

            
            return RedirectToPage("/Index");
        }
    }
}