using KursachBD.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KursachBD.Pages
{
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CartModel : PageModel
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;
        private readonly CartService _cartService;
        private readonly IAntiforgery _antiforgery;

        public CartModel(
            OnlineStoreSellingComputerEquipmentContext context,
            CartService cartService,
            IAntiforgery antiforgery)
        {
            _context = context;
            _cartService = cartService;
            _antiforgery = antiforgery;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalWeight { get; set; }

        public string CurrentCustomerAddress { get; set; } = string.Empty;

        [BindProperty]
        public string DeliveryAddress { get; set; } = string.Empty;

        [BindProperty]
        public string PaymentMethod { get; set; } = string.Empty;

        [BindProperty]
        public long ProductId { get; set; }

        [BindProperty]
        public int Change { get; set; }

        public class CartItemViewModel
        {
            public long ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal Weight { get; set; }
            public int Quantity { get; set; }
            public int Stock { get; set; }
        }

        public async Task OnGetAsync()
        {
            var customerId = GetCurrentCustomerId();
            CurrentCustomerAddress = string.Empty;
            await LoadCartItems(customerId);
        }

        public async Task<IActionResult> OnPostAdjustQuantityAsync()
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var success = await _cartService.AdjustCartItemQuantityAsync(customerId, ProductId, Change);

                if (!success) return new JsonResult(new { success = false, message = "Ошибка изменения количества" });

                await LoadCartItems(customerId); // Пересчитываем цены и доставку
                var updatedItem = CartItems.FirstOrDefault(i => i.ProductId == ProductId);
                var productInfo = await _context.Products.FindAsync(ProductId);
                var cartCount = await _cartService.GetCartCountAsync(customerId);

                var result = new
                {
                    success = true,
                    itemRemoved = updatedItem == null,
                    // Данные по конкретному товару
                    itemTotal = updatedItem != null ? (updatedItem.Price * updatedItem.Quantity).ToString("N2") : "0",
                    itemWeight = updatedItem != null ? (updatedItem.Weight * updatedItem.Quantity).ToString("N3") : "0",
                    currentStock = productInfo?.Stock ?? 0,

                    // Общие данные корзины
                    subTotal = SubTotal.ToString("N2"),
                    total = Total.ToString("N2"),
                    totalWeight = TotalWeight.ToString("N3"),
                    shippingCost = ShippingCost.ToString("N2"), // Передаем стоимость доставки
                    cartCount = cartCount
                };

                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        public async Task<IActionResult> OnPostRemoveFromCartAsync()
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                await _cartService.RemoveFromCartCompletelyAsync(customerId, ProductId);
                await LoadCartItems(customerId); // Пересчитываем
                var productInfo = await _context.Products.FindAsync(ProductId);
                var cartCount = await _cartService.GetCartCountAsync(customerId);

                return new JsonResult(new
                {
                    success = true,
                    subTotal = SubTotal.ToString("N2"),
                    total = Total.ToString("N2"),
                    totalWeight = TotalWeight.ToString("N3"),
                    shippingCost = ShippingCost.ToString("N2"), // Передаем стоимость доставки
                    currentStock = productInfo?.Stock ?? 0,
                    cartCount = cartCount
                });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        public async Task<IActionResult> OnPostClearCartAsync()
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                var success = await _cartService.ClearCartAsync(customerId);
                return new JsonResult(new { success, message = success ? "Очищено" : "Корзина пуста" });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = ex.Message }); }
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                if (string.IsNullOrWhiteSpace(DeliveryAddress))
                    return new JsonResult(new { success = false, message = "Адрес доставки обязателен." });

                var success = await _cartService.CheckoutAsync(customerId, DeliveryAddress, PaymentMethod);

                if (success)
                    return new JsonResult(new { success = true, message = "Заказ успешно оформлен!" });
                else
                    return new JsonResult(new { success = false, message = "Не удалось оформить заказ." });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = "Ошибка: " + ex.Message }); }
        }

        public async Task<IActionResult> OnGetAntiforgeryTokenAsync()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return new JsonResult(new { token = tokens.RequestToken });
        }

        private async Task LoadCartItems(long customerId)
        {
            var activeOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);

            if (activeOrder == null)
            {
                CartItems = new List<CartItemViewModel>();
                SubTotal = 0;
                TotalWeight = 0;
                ShippingCost = 0;
                Total = 0;
                TotalItems = 0;
                return;
            }

            var cartItems = await _context.ContentOfOrders
                .Where(co => co.NumberOfOrder == activeOrder.NumberOfOrder)
                .Select(co => new { co.ProductId, co.Price, co.Quantity, Product = co.Product })
                .ToListAsync();

            CartItems = cartItems.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Товар",
                Manufacturer = item.Product?.Manufacturer ?? "",
                Price = item.Price,
                Weight = item.Product?.Weight ?? 0,
                Quantity = (int)item.Quantity,
                Stock = (int)(item.Product?.Stock ?? 0)
            }).ToList();

            TotalItems = CartItems.Sum(i => i.Quantity);
            SubTotal = CartItems.Sum(i => i.Price * i.Quantity);
            TotalWeight = CartItems.Sum(i => i.Weight * i.Quantity);

            
            if (TotalWeight > 100 || SubTotal > 20000)
            {
                ShippingCost = 500;
            }
            else
            {
                ShippingCost = 0;
            }

            Total = SubTotal + ShippingCost;
        }

        private long GetCurrentCustomerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        }
    }
}