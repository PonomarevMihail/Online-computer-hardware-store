using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

public partial class ContentOfOrder
{
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Номер заказа должен быть положительным числом.")]
    public long NumberOfOrder { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Идентификатор продукта должен быть положительным числом.")]
    public long ProductId { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Количество товара должно быть больше нуля.")]
    public long Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной.")]
    public decimal Price { get; set; }

    [ForeignKey(nameof(NumberOfOrder))]
    public virtual Order NumberOfOrderNavigation { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}