using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

[Table("product_price_history", Schema = "store")]
public partial class ProductPriceHistory
{
    [Key]
    [Range(1, long.MaxValue, ErrorMessage = "Идентификатор записи истории должен быть положительным числом.")]
    public long HistoryId { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Идентификатор товара должен быть положительным числом.")]
    [ForeignKey(nameof(Product))]
    public long ProductId { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    [Range(0, 10000000, ErrorMessage = "Старая цена должна быть от 0 до 10 000 000.")]
    public decimal OldPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(12,2)")]
    [Range(0.01, 10000000, ErrorMessage = "Новая цена должна быть от 0.01 до 10 000 000.")]
    public decimal NewPrice { get; set; }

    public DateTime? ChangedAt { get; set; }

    [StringLength(100, ErrorMessage = "Имя пользователя, изменившего цену, не может превышать 100 символов.")]
    public string? ChangedBy { get; set; }

    public virtual Product Product { get; set; } = null!;
}