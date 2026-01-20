using KursachBD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;
using System.Text.Json; 

namespace KursachBD.Pages
{
    public class CatalogModel : PageModel
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;
        private readonly CartService _cartService;

        public CatalogModel(OnlineStoreSellingComputerEquipmentContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public List<Models.Product> Products { get; set; } = new List<Models.Product>();
        public Dictionary<string, string> CategoryNames { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<CategoryItem>> GroupedCategories { get; set; } = new();

        public string? SelectedGroup { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SortBy { get; set; }
        public string? SearchQuery { get; set; }

        public int TotalProducts { get; set; }
        public bool IsAdmin { get; set; } = false;

        // Фильтры
        public Dictionary<string, List<string>> AvailableFilters { get; set; } = new();
        public Dictionary<string, string> SelectedSpecs { get; set; } = new();

        public class CategoryItem
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

       
        public class SalesStatDto
        {
            public DateTime Period_start { get; set; }
            public DateTime Period_end { get; set; }
            public long Total_orders { get; set; }
            public decimal Total_revenue { get; set; }
            public decimal Average_order_value { get; set; }
            public long Total_products_sold { get; set; }
            public string? Most_popular_product { get; set; }
            public string? Most_popular_category { get; set; }
        }

        public class CustomerPurchaseDto
        {
            public string Customer_name { get; set; }
            public string Customer_email { get; set; }
            public long Order_number { get; set; }
            public DateTime Order_date { get; set; }
            public string Product_name { get; set; }
            public string Product_type { get; set; }
            public long Quantity { get; set; }
            public decimal Total_price { get; set; }
        }

        public async Task OnGetAsync(string? group, string? category, string? sort, string? search)
        {
            SelectedGroup = group;
            SelectedCategory = category;
            SortBy = sort ?? "new";
            SearchQuery = search;

            await CheckAdminStatus();
            await LoadCategories();

            // Считываем выбранные параметры фильтров из URL
            foreach (var key in Request.Query.Keys)
            {
                if (key.StartsWith("spec_"))
                {
                    var specName = key.Substring(5);
                    var specValue = Request.Query[key].ToString();
                    if (!string.IsNullOrEmpty(specValue))
                    {
                        SelectedSpecs[specName] = specValue;
                    }
                }
            }

            await LoadProductsAndFilters();
        }

        private async Task LoadProductsAndFilters()
        {
            IQueryable<Models.Product> query = _context.Products.Include(p => p.Reviews).AsNoTracking();

            // Базовая фильтрация по группе/категории
            if (!string.IsNullOrEmpty(SelectedGroup) && GroupedCategories.ContainsKey(SelectedGroup))
            {
                var categoriesInGroup = GroupedCategories[SelectedGroup].Select(c => c.Id).ToList();
                if (!string.IsNullOrEmpty(SelectedCategory) && categoriesInGroup.Contains(SelectedCategory))
                    query = query.Where(p => p.TypeOfProduct.ToString() == SelectedCategory);
                else
                    query = query.Where(p => categoriesInGroup.Contains(p.TypeOfProduct.ToString()));
            }
            else if (!string.IsNullOrEmpty(SelectedCategory))
            {
                query = query.Where(p => p.TypeOfProduct.ToString() == SelectedCategory);
            }

            // Поиск
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var searchLower = SearchQuery.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower) || (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(searchLower)));
            }

            // Получаем список из БД
            var initialProducts = await query.ToListAsync();
            List<Models.Product> finalProducts;

            //Генерируем фильтры по характеристикам
            if (!string.IsNullOrEmpty(SelectedCategory) || !string.IsNullOrEmpty(SelectedGroup))
            {
                GenerateAvailableFilters(initialProducts);
                finalProducts = ApplySpecFilters(initialProducts);
            }
            else
            {
                
                finalProducts = initialProducts;
            }

            // Сортировка
            finalProducts = SortBy switch
            {
                "price_asc" => finalProducts.OrderBy(p => p.Price).ToList(),
                "price_desc" => finalProducts.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => finalProducts.OrderBy(p => p.Name).ToList(),
                "name_desc" => finalProducts.OrderByDescending(p => p.Name).ToList(),
                "new" => finalProducts.OrderByDescending(p => p.ProductId).ToList(),
                _ => finalProducts.OrderBy(p => p.ProductId).ToList()
            };

            Products = finalProducts;
            TotalProducts = Products.Count;
        }

        private void GenerateAvailableFilters(List<Models.Product> products)
        {
            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.Characteristics)) continue;

                try
                {
                    var specs = JsonSerializer.Deserialize<Dictionary<string, string>>(product.Characteristics);
                    if (specs == null) continue;

                    foreach (var spec in specs)
                    {
                        var key = spec.Key.Trim();
                        var value = spec.Value.Trim();

                        if (!AvailableFilters.ContainsKey(key))
                        {
                            AvailableFilters[key] = new List<string>();
                        }

                        if (!AvailableFilters[key].Contains(value))
                        {
                            AvailableFilters[key].Add(value);
                        }
                    }
                }
                catch { }
            }

            foreach (var key in AvailableFilters.Keys.ToList())
            {
                AvailableFilters[key].Sort();
            }
        }

        private List<Models.Product> ApplySpecFilters(List<Models.Product> products)
        {
            if (SelectedSpecs.Count == 0) return products;

            var result = new List<Models.Product>();

            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.Characteristics)) continue;

                try
                {
                    var productSpecs = JsonSerializer.Deserialize<Dictionary<string, string>>(product.Characteristics);
                    if (productSpecs == null) continue;

                    bool matchAll = true;

                    foreach (var selectedFilter in SelectedSpecs)
                    {
                        if (!productSpecs.ContainsKey(selectedFilter.Key) ||
                            productSpecs[selectedFilter.Key] != selectedFilter.Value)
                        {
                            matchAll = false;
                            break;
                        }
                    }

                    if (matchAll) result.Add(product);
                }
                catch { }
            }

            return result;
        }

        private async Task CheckAdminStatus()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                try
                {
                    var userId = GetCurrentCustomerId();
                    var user = await _context.Customers.FindAsync(userId);
                    if (user != null && user.RoleId == 1) IsAdmin = true;
                }
                catch { IsAdmin = false; }
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(long productId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return new JsonResult(new { success = false, message = "Необходимо войти в систему" });

            try
            {
                var customerId = GetCurrentCustomerId();
                await _cartService.AddToCartAsync(customerId, productId);

                // Получаем свежие данные для обновления интерфейса
                var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
                var activeOrder = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerId == customerId && o.PaymentDate == null);

                // Сколько теперь этого товара в корзине?
                var itemInCart = await _context.ContentOfOrders
                    .FirstOrDefaultAsync(co => co.NumberOfOrder == activeOrder.NumberOfOrder && co.ProductId == productId);

                int quantityInCart = (int)(itemInCart?.Quantity ?? 0);
                int maxStock = (int)(product?.Stock ?? 0);

                var cartCount = await _cartService.GetCartCountAsync(customerId);

                return new JsonResult(new
                {
                    success = true,
                    message = "Товар добавлен в корзину!",
                    cartCount = cartCount,
                    
                    quantityInCart = quantityInCart,
                    maxStock = maxStock
                });
            }
            catch (Exception ex)
            {
                
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostAddReviewAsync(long productId, string reviewText, int rating)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return new JsonResult(new { success = false, message = "Необходимо войти в систему" });

            if (string.IsNullOrWhiteSpace(reviewText) || reviewText.Trim().Length < 10)
                return new JsonResult(new { success = false, message = "Отзыв слишком короткий." });

            try
            {
                var customerId = GetCurrentCustomerId();
                var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.CustomerId == customerId && r.ProductId == productId);
                if (existingReview != null) return new JsonResult(new { success = false, message = "Вы уже оставили отзыв." });

                var review = new Review { Review1 = reviewText, Rating = rating, CustomerId = customerId, ProductId = productId, CreatedAt = DateTime.Now };
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, message = "Отзыв добавлен!" });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = "Ошибка: " + ex.Message }); }
        }

        public async Task<IActionResult> OnPostDeleteReviewAsync(long reviewId)
        {
            await CheckAdminStatus();
            if (!IsAdmin) return new JsonResult(new { success = false, message = "Нет прав." });

            try
            {
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.NumberOfReview == (int)reviewId);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                    return new JsonResult(new { success = true, message = "Отзыв удален." });
                }
                return new JsonResult(new { success = false, message = "Отзыв не найден." });
            }
            catch (Exception ex) { return new JsonResult(new { success = false, message = "Ошибка: " + ex.Message }); }
        }

        public async Task<IActionResult> OnGetSalesStatsAsync(string start, string end)
        {
            await CheckAdminStatus();
            if (!IsAdmin) return new JsonResult(new { success = false, message = "Нет прав." });

            try
            {
                if (!DateTime.TryParse(start, out DateTime startDate) || !DateTime.TryParse(end, out DateTime endDate))
                    return new JsonResult(new { success = false, message = "Неверный формат даты." });

                var result = await _context.Database
                    .SqlQuery<SalesStatDto>($"SELECT * FROM get_sales_statistics({startDate}::date, {endDate}::date)")
                    .ToListAsync();

                return new JsonResult(new { success = true, data = result.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Ошибка SQL: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        public async Task<IActionResult> OnGetCustomerHistoryAsync(string? email)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return new JsonResult(new { success = false, message = "Необходимо войти в систему" });

            try
            {
                var currentUserId = GetCurrentCustomerId();
                bool isAdmin = false;
                var user = await _context.Customers.FindAsync(currentUserId);
                if (user != null && user.RoleId == 1) isAdmin = true;

                List<CustomerPurchaseDto> result;

                if (isAdmin && !string.IsNullOrWhiteSpace(email))
                {
                    result = await _context.Database
                        .SqlQuery<CustomerPurchaseDto>($"SELECT * FROM get_customer_purchases(NULL, {email})")
                        .ToListAsync();
                }
                else
                {
                    result = await _context.Database
                        .SqlQuery<CustomerPurchaseDto>($"SELECT * FROM get_customer_purchases({currentUserId}::bigint, NULL)")
                        .ToListAsync();
                }

                return new JsonResult(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Ошибка: " + ex.Message });
            }
        }

        private async Task LoadCategories()
        {
            CategoryNames = GetCategoryNamesDictionary();
            GroupedCategories = GetGroupedCategories();
            if (!string.IsNullOrEmpty(SelectedGroup) && string.IsNullOrEmpty(SelectedCategory))
            {
                if (GroupedCategories.TryGetValue(SelectedGroup, out var categoriesInGroup) && categoriesInGroup.Any())
                    SelectedCategory = categoriesInGroup.First().Id;
            }
        }

        private Dictionary<string, string> GetCategoryNamesDictionary()
        {
            return new Dictionary<string, string>
            {
                { "laptop", "Ноутбуки" }, { "desktop", "Настольные компьютеры" }, { "all_in_one", "Моноблоки" },
                { "workstation", "Рабочие станции" }, { "server", "Серверы" }, { "processor", "Процессоры" },
                { "motherboard", "Материнские платы" }, { "ram", "Оперативная память" }, { "video_card", "Видеокарты" },
                { "power_supply", "Блоки питания" }, { "computer_case", "Корпусы" }, { "cooling_system", "Системы охлаждения" },
                { "ssd", "SSD накопители" }, { "hdd", "Жесткие диски" }, { "external_drive", "Внешние накопители" },
                { "flash_drive", "Флеш-накопители" }, { "memory_card", "Карты памяти" }, { "monitor", "Мониторы" },
                { "keyboard", "Клавиатуры" }, { "mouse", "Мыши" }, { "headphones", "Наушники" }, { "webcam", "Веб-камеры" },
                { "microphone", "Микрофоны" }, { "speakers", "Колонки" }, { "router", "Маршрутизаторы" },
                { "network_adapter", "Сетевые адаптеры" }, { "wifi_extender", "Wi-Fi усилители" }, { "printer", "Принтеры" },
                { "scanner", "Сканеры" }, { "multifunction_device", "МФУ" }, { "mouse_pad", "Коврики для мыши" },
                { "usb_hub", "USB-хабы" }, { "cable", "Кабели" }, { "adapter", "Адаптеры" }, { "laptop_bag", "Сумки для ноутбуков" },
                { "gaming_mouse", "Игровые мыши" }, { "gaming_keyboard", "Игровые клавиатуры" }, { "gamepad", "Геймпады" },
                { "joystick", "Джойстики" }, { "vr_headset", "VR очки" }, { "software", "Программное обеспечение" },
                { "component", "Комплектующие" }, { "accessory", "Аксессуары" }
            };
        }

        public Dictionary<string, List<CategoryItem>> GetGroupedCategories()
        {
            return new Dictionary<string, List<CategoryItem>>
            {
                { "computer_systems", new List<CategoryItem> { new CategoryItem { Id = "laptop", Name = "Ноутбуки" }, new CategoryItem { Id = "desktop", Name = "Настольные ПК" }, new CategoryItem { Id = "all_in_one", Name = "Моноблоки" }, new CategoryItem { Id = "workstation", Name = "Рабочие станции" }, new CategoryItem { Id = "server", Name = "Серверы" } } },
                { "pc_components", new List<CategoryItem> { new CategoryItem { Id = "processor", Name = "Процессоры" }, new CategoryItem { Id = "motherboard", Name = "Материнские платы" }, new CategoryItem { Id = "ram", Name = "Оперативная память" }, new CategoryItem { Id = "video_card", Name = "Видеокарты" }, new CategoryItem { Id = "power_supply", Name = "Блоки питания" }, new CategoryItem { Id = "computer_case", Name = "Корпусы" }, new CategoryItem { Id = "cooling_system", Name = "Системы охлаждения" } } },
                { "storage", new List<CategoryItem> { new CategoryItem { Id = "ssd", Name = "SSD накопители" }, new CategoryItem { Id = "hdd", Name = "Жесткие диски" }, new CategoryItem { Id = "external_drive", Name = "Внешние накопители" }, new CategoryItem { Id = "flash_drive", Name = "Флеш-накопители" }, new CategoryItem { Id = "memory_card", Name = "Карты памяти" } } },
                { "peripherals", new List<CategoryItem> { new CategoryItem { Id = "monitor", Name = "Мониторы" }, new CategoryItem { Id = "keyboard", Name = "Клавиатуры" }, new CategoryItem { Id = "mouse", Name = "Мыши" }, new CategoryItem { Id = "headphones", Name = "Наушники" }, new CategoryItem { Id = "webcam", Name = "Веб-камеры" }, new CategoryItem { Id = "microphone", Name = "Микрофоны" }, new CategoryItem { Id = "speakers", Name = "Колонки" } } },
                { "networking", new List<CategoryItem> { new CategoryItem { Id = "router", Name = "Маршрутизаторы" }, new CategoryItem { Id = "network_adapter", Name = "Сетевые адаптеры" }, new CategoryItem { Id = "wifi_extender", Name = "Wi-Fi усилители" } } },
                { "office_equipment", new List<CategoryItem> { new CategoryItem { Id = "printer", Name = "Принтеры" }, new CategoryItem { Id = "scanner", Name = "Сканеры" }, new CategoryItem { Id = "multifunction_device", Name = "МФУ" } } },
                { "gaming_peripherals", new List<CategoryItem> { new CategoryItem { Id = "gaming_mouse", Name = "Игровые мыши" }, new CategoryItem { Id = "gaming_keyboard", Name = "Игровые клавиатуры" }, new CategoryItem { Id = "gamepad", Name = "Геймпады" }, new CategoryItem { Id = "joystick", Name = "Джойстики" }, new CategoryItem { Id = "vr_headset", Name = "VR очки" } } },
                { "accessories", new List<CategoryItem> { new CategoryItem { Id = "mouse_pad", Name = "Коврики для мыши" }, new CategoryItem { Id = "usb_hub", Name = "USB-хабы" }, new CategoryItem { Id = "cable", Name = "Кабели" }, new CategoryItem { Id = "adapter", Name = "Адаптеры" }, new CategoryItem { Id = "laptop_bag", Name = "Сумки для ноутбуков" } } },
                { "other", new List<CategoryItem> { new CategoryItem { Id = "software", Name = "Программное обеспечение" }, new CategoryItem { Id = "component", Name = "Комплектующие" }, new CategoryItem { Id = "accessory", Name = "Аксессуары" } } }
            };
        }

        public string GetGroupDisplayName(string groupId)
        {
            return groupId switch
            {
                "computer_systems" => "Компьютерные системы",
                "pc_components" => "Комплектующие ПК",
                "storage" => "Накопители",
                "peripherals" => "Периферия",
                "networking" => "Сетевое оборудование",
                "office_equipment" => "Офисная техника",
                "gaming_peripherals" => "Игровая периферия",
                "accessories" => "Аксессуары",
                "other" => "Прочее",
                _ => groupId
            };
        }

        public string GetCategoryDisplayName(string category)
        {
            return CategoryNames.TryGetValue(category, out var displayName) ? displayName : category;
        }

        private long GetCurrentCustomerId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long id)) return id;
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        }
    }
}