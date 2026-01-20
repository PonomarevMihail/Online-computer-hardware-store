using KursachBD.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

public partial class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ProductId { get; set; }

    [Required(ErrorMessage = "Название товара обязательно.")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Название товара должно содержать от 2 до 255 символов.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Характеристики товара обязательны.")]
    [StringLength(2000, MinimumLength = 5, ErrorMessage = "Характеристики должны содержать от 5 до 2000 символов.")]
    public string Characteristics { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    [Range(100, 10000000, ErrorMessage = "Цена товара должна быть от 0.01 до 10 000 000.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Производитель обязателен.")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Производитель должен содержать от 2 до 255 символов.")]
    public string Manufacturer { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(5,3)")]
    [Range(0.001, 100, ErrorMessage = "Вес товара должен быть от 0.001 до 100 кг.")]
    public decimal Weight { get; set; }

    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Количество на складе не может быть отрицательным.")]
    public long Stock { get; set; }

    [Required]
    [Column("type_of_product")]
    public ProductType TypeOfProduct { get; set; }
    public virtual ICollection<ContentOfOrder> ContentOfOrders { get; set; } = new List<ContentOfOrder>();
    public virtual ICollection<DeliveryNoteContent> DeliveryNoteContents { get; set; } = new List<DeliveryNoteContent>();
    public virtual ICollection<ProductPriceHistory> ProductPriceHistories { get; set; } = new List<ProductPriceHistory>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

