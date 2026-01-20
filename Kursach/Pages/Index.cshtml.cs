using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KursachBD.Models; // Убедитесь, что здесь пространство имен, где лежит ваш Context
using System.Security.Claims;

namespace KursachBD.Pages
{
    public class IndexModel : PageModel
    {
        
        private readonly OnlineStoreSellingComputerEquipmentContext _context;
        private readonly CartService _cartService;

        
        public IndexModel(OnlineStoreSellingComputerEquipmentContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        
        public List<Models.Product> BestSellers { get; set; } = new List<Models.Product>();
        public List<Models.Product> NewProducts { get; set; } = new List<Models.Product>();
        public List<Models.Product> FeaturedProducts { get; set; } = new List<Models.Product>();

        public int TotalProducts { get; set; }
        public int CartItemCount { get; set; } = 0;

        
        public async Task OnGetAsync()
        {
            // Загружаем товары, которые физически есть на складе (stock > 0)
            BestSellers = await _context.Products
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.Stock > 0)
                .Take(8)
                .ToListAsync();

            NewProducts = await _context.Products
                .OrderByDescending(p => p.ProductId)
                .Where(p => p.Stock > 0)
                .Take(4)
                .ToListAsync();

            FeaturedProducts = await _context.Products
                .Where(p => p.Stock > 0)
                .Take(4)
                .ToListAsync();

            TotalProducts = await _context.Products.CountAsync(p => p.Stock > 0);

            // Получаем кол-во товаров в корзине (для иконки в шапке)
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long customerId))
                {
                    CartItemCount = await _cartService.GetCartCountAsync(customerId);
                }
            }
        }

        // Метод добавления в корзину 
        public async Task<IActionResult> OnPostAddToCartAsync(long productId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return new JsonResult(new { success = false, message = "Необходимо войти в систему" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long customerId))
            {
                try
                {
                    // Быстрая проверка "на глаз"
                    var product = await _context.Products.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == productId);

                    if (product == null || product.Stock <= 0)
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            message = "Товар закончился.",
                            isLimitReached = true
                        });
                    }

                    // Попытка добавления через сервис
                    // Если сработает триггер stock_control (недостаточно товара), сервис выкинет ошибку
                    await _cartService.AddToCartAsync(customerId, productId);

                    // Если всё ок - обновляем счетчик
                    var newCount = await _cartService.GetCartCountAsync(customerId);
                    return new JsonResult(new { success = true, message = "Товар добавлен", cartCount = newCount });
                }
                catch (Exception ex)
                {
                    
                    if (ex.Message.Contains("Недостаточно товара") || ex.Message.Contains("лимит"))
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            message = "Вы достигли лимита доступного товара.",
                            isLimitReached = true
                        });
                    }
                    return new JsonResult(new { success = false, message = ex.Message });
                }
            }

            return new JsonResult(new { success = false, message = "Ошибка идентификации" });
        }
    }
}