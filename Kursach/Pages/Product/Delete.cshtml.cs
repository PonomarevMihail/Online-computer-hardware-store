using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KursachBD.Models;
using Npgsql; // Обязательно добавьте эту директиву для обработки ошибок Postgres

namespace KursachBD.Pages.Product
{
    public class DeleteModel : PageModel
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;

        public DeleteModel(OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Product Product { get; set; } = default!;

        // Свойство для вывода ошибки на страницу
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(long? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;

            if (saveChangesError.GetValueOrDefault())
            {
                ErrorMessage = "Не удалось удалить товар. Возможно, он уже содержится в заказах или имеет отзывы.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                try
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Проверяем, является ли ошибка нарушением внешнего ключа (код 23503 в Postgres)
                    if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
                    {
                        // Перенаправляем обратно на эту же страницу с флагом ошибки
                        return RedirectToPage("./Delete", new { id, saveChangesError = true });
                    }
                    else
                    {
                        // Если ошибка другая, выбрасываем её дальше
                        throw;
                    }
                }
            }

            return RedirectToPage("./Index");
        }
    }
}