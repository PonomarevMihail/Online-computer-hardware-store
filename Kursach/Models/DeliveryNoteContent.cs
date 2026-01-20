using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

public partial class DeliveryNoteContent
{
    [Key]
    public long IdNc { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Количество должно быть больше нуля.")]
    public long Quantity { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Идентификатор заголовка накладной должен быть положительным числом.")]
    public long IdNh { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Идентификатор продукта должен быть положительным числом.")]
    public long ProductId { get; set; }

    [ForeignKey(nameof(IdNh))]
    public virtual DeliveryNoteHead IdNhNavigation { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;
}
