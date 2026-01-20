using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

public partial class DeliveryNoteHead
{
    [Key]
    public long IdNh { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Грузоотправитель не может превышать 200 символов.")]
    public string Shipper { get; set; } = null!;

    [Required]
    [StringLength(200, ErrorMessage = "Грузополучатель не может превышать 200 символов.")]
    public string Consignee { get; set; } = null!;

    [Required]
    [StringLength(200, ErrorMessage = "Продавец не может превышать 200 символов.")]
    public string Salesman { get; set; } = null!;

    [Required]
    [StringLength(200, ErrorMessage = "Покупатель не может превышать 200 символов.")]
    public string Buyer { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Итоговая сумма должна быть положительной.")]
    public decimal SummaryPrice { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly DeliveryDate { get; set; }

    [ForeignKey(nameof(Buyer))]
    public virtual LegalEntity BuyerNavigation { get; set; } = null!;

    [ForeignKey(nameof(Consignee))]
    public virtual LegalEntity ConsigneeNavigation { get; set; } = null!;

    public virtual ICollection<DeliveryNoteContent> DeliveryNoteContents { get; set; } = new List<DeliveryNoteContent>();

    [ForeignKey(nameof(Salesman))]
    public virtual LegalEntity SalesmanNavigation { get; set; } = null!;

    [ForeignKey(nameof(Shipper))]
    public virtual LegalEntity ShipperNavigation { get; set; } = null!;
}
