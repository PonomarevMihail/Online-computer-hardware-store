using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KursachBD.Models;

namespace KursachBD.Pages.Product
{
    public class DetailsModel : PageModel
    {
        private readonly OnlineStoreSellingComputerEquipmentContext _context;

        public DetailsModel(OnlineStoreSellingComputerEquipmentContext context)
        {
            _context = context;
        }

        public Models.Product Product { get; set; } = default!;

        // ВОТ ЭТО СВОЙСТВО ДОЛЖНО БЫТЬ ОБЯЗАТЕЛЬНО:
        public string ProductTypeRus { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();

            Product = product;

            // ЗДЕСЬ МЫ ЗАПОЛНЯЕМ РУССКОЕ НАЗВАНИЕ:
            ProductTypeRus = GetCategoryName(Product.TypeOfProduct.ToString());

            return Page();
        }

        // Словарь для перевода
        private string GetCategoryName(string type)
        {
            var dict = new Dictionary<string, string>
            {
                 { "laptop", "Ноутбуки" }, { "desktop", "Настольные компьютеры" }, { "all_in_one", "Моноблоки" },
                 { "workstation", "Рабочие станции" }, { "server", "Серверы" }, { "processor", "Процессоры" },
                 { "motherboard", "Материнские платы" }, { "ram", "Оперативная память" }, { "video_card", "Видеокарты" },
                 { "power_supply", "Блоки питания" }, { "computer_case", "Корпуса" }, { "cooling_system", "Системы охлаждения" },
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

            // Если нашли в словаре - возвращаем русское, если нет - возвращаем как есть
            return dict.ContainsKey(type) ? dict[type] : type;
        }
    }
}