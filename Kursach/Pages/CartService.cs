using KursachBD.Models;
using Microsoft.EntityFrameworkCore;

namespace KursachBD.Pages
{
    public class CartService
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(OnlineStoreSellingComputerEquipmentContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- Добавление товара ---
        public async Task<bool> AddToCartAsync(long customerId, long productId, int quantity = 1)
        {
            // Начинаем транзакцию, чтобы не создавать пустой заказ, если товар добавить нельзя
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Получаем или готовим (но пока не сохраняем) заказ
                var activeOrder = await GetActiveOrderInternalAsync(customerId);
                bool isNewOrder = activeOrder.NumberOfOrder == 0; 

                if (isNewOrder)
                {
                    _context.Orders.Add(activeOrder);
                    // Важно: Сохраняем, чтобы получить ID заказа для связки с товаром
                    await _context.SaveChangesAsync();
                }

                // 2. Проверяем товар
                var product = await _context.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null) throw new Exception("Товар не найден");

                // 3. Ищем, есть ли товар в корзине
                var existingItem = await _context.ContentOfOrders
                    .FirstOrDefaultAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder && co.ProductId == productId);

                int currentInCart = (int)(existingItem?.Quantity ?? 0);

                // 4. ПРОВЕРКА ЛИМИТА
                if (currentInCart + quantity > product.Stock)
                {
                 
                  throw new Exception($"Достигнут лимит товара. В наличии: {product.Stock}, в корзине: {currentInCart}");
                }

                // 5. Добавляем или обновляем позицию
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.Price = product.Price;
                }
                else
                {
                    _context.ContentOfOrders.Add(new ContentOfOrder
                    {
                        NumberOfOrder = activeOrder.NumberOfOrder,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    });
                }

                // 6. Сохраняем содержимое и фиксируем транзакцию
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Откат транзакции, если словили ошибку
                await transaction.RollbackAsync();

                _logger.LogError(ex, "Ошибка при добавлении в корзину");
                throw; 
            }
        }

        // --- Изменение количества ---
        public async Task<bool> AdjustCartItemQuantityAsync(long customerId, long productId, int change)
        {
            try
            {
                var activeOrder = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);
                if (activeOrder == null) return false;

                var cartItem = await _context.ContentOfOrders
                    .Include(co => co.Product)
                    .FirstOrDefaultAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder && co.ProductId == productId);

                if (cartItem == null) return false;

                var newQuantity = (int)cartItem.Quantity + change;

                // Проверка лимита при увеличении
                if (change > 0 && newQuantity > cartItem.Product.Stock)
                {
                    throw new Exception($"Нельзя добавить больше. На складе всего: {cartItem.Product.Stock} шт.");
                }

                if (newQuantity < 1)
                {
                    _context.ContentOfOrders.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = newQuantity;
                }

                await _context.SaveChangesAsync();

                // Удаляем пустой заказ (чистим мусор)
                if (!await _context.ContentOfOrders.AnyAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder))
                {
                    _context.Orders.Remove(activeOrder);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка изменения количества");
                throw;
            }
        }

        // --- Полное удаление товара ---
        public async Task<bool> RemoveFromCartCompletelyAsync(long customerId, long productId)
        {
            try
            {
                var activeOrder = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);
                if (activeOrder == null) return false;

                var cartItem = await _context.ContentOfOrders
                    .FirstOrDefaultAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder && co.ProductId == productId);

                if (cartItem == null) return false;

                _context.ContentOfOrders.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Чистим пустую корзину
                if (!await _context.ContentOfOrders.AnyAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder))
                {
                    _context.Orders.Remove(activeOrder);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch { return false; }
        }

        // --- Очистка корзины ---
        public async Task<bool> ClearCartAsync(long customerId)
        {
            try
            {
                var activeOrder = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);
                if (activeOrder == null) return false;

                
                _context.Orders.Remove(activeOrder); 

                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        // --- Оформление заказа  ---
        public async Task<bool> CheckoutAsync(long customerId, string deliveryAddress, string paymentMethod)
        {
            // Используем транзакцию для атомарности операции "Превращение корзины в заказ"
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var activeOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);

                if (activeOrder == null || !await _context.ContentOfOrders.AnyAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder))
                    return false;

                // 1. Обновляем данные (пока только в памяти)
                activeOrder.DeliveryAddress = deliveryAddress;

                string methodText = paymentMethod switch { "card" => "Карта", "sbp" => "СБП", "cash" => "Наличные", _ => paymentMethod };
                activeOrder.Comment = string.IsNullOrEmpty(activeOrder.Comment) ? $"Оплата: {methodText}" : $"{activeOrder.Comment}. Оплата: {methodText}";

                // 2. Устанавливаем дату оплаты -> ЭТО ЗАПУСКАЕТ ТРИГГЕР в БД
                activeOrder.PaymentDate = DateTime.Now;

                // 3. Пытаемся сохранить
                // Если триггер 'trg_deduct_stock_on_payment' выбросит ошибку (нет товара),
                // здесь вылетит DbUpdateException.
                await _context.SaveChangesAsync();

                // 4. Если успех - фиксируем изменения
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // ОТКАТЫВАЕМ ТРАНЗАКЦИЮ
                await transaction.RollbackAsync();

                var sqlError = ex.InnerException?.Message ?? ex.Message;
                if (sqlError.Contains("Недостаточно товара") || sqlError.Contains("Товар ID"))
                {
                    throw new Exception($"Не удалось оформить заказ: {sqlError}");
                }
                throw new Exception("Ошибка базы данных при оформлении: " + sqlError);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Checkout error");
                throw;
            }
        }

        public async Task<int> GetCartCountAsync(long customerId)
        {
            var activeOrder = await _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);

            if (activeOrder == null) return 0;

            return await _context.ContentOfOrders
                .Where(co => co.NumberOfOrder == activeOrder.NumberOfOrder)
                .SumAsync(co => (int?)co.Quantity) ?? 0;
        }

        // --- Вспомогательный метод ---
        private async Task<KursachBD.Models.Order> GetActiveOrderInternalAsync(long customerId)
        {
            var activeOrder = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);

            if (activeOrder == null)
            {
                // Мы создаем объект в памяти, но НЕ добавляем в контекст и НЕ сохраняем здесь.
                // Это делает вызывающий метод внутри транзакции.
                activeOrder = new KursachBD.Models.Order
                {
                    CustomerId = customerId,
                    DeliveryAddress = "Не указано",
                    Price = 0,
                    Comment = "",
                    PaymentDate = null,
                    DeliveryDate = DateTime.Now.AddDays(2)
                };
            }
            return activeOrder;
        }
    }
}