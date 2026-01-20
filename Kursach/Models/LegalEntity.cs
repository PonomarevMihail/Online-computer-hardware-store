using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KursachBD.Models;

public partial class LegalEntity
{
    [Key]
    [Required(ErrorMessage = "ИНН обязателен.")]
    [RegularExpression(@"^\d{10}$|^\d{12}$", ErrorMessage = "ИНН должен содержать 10 или 12 цифр.")]
    public string Inn { get; set; } = null!;

    [Required(ErrorMessage = "Имя обязательно.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 100 символов.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Фамилия обязательна.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 100 символов.")]
    public string Surname { get; set; } = null!;

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Отчество должно содержать от 2 до 100 символов.")]
    public string? Patronymic { get; set; }

    [Required(ErrorMessage = "Компания обязательна.")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Название компании должно содержать от 2 до 255 символов.")]
    public string Company { get; set; } = null!;

    [Required(ErrorMessage = "Телефон обязателен.")]
    [RegularExpression(@"^(?:\+7|8)\d{10}$", ErrorMessage = "Номер телефона должен быть в формате +7XXXXXXXXXX или 8XXXXXXXXXX.")]
    [StringLength(12)]
    public string? Phone { get; set; }

    public virtual ICollection<DeliveryNoteHead> DeliveryNoteHeadBuyerNavigations { get; set; } = new List<DeliveryNoteHead>();

    public virtual ICollection<DeliveryNoteHead> DeliveryNoteHeadConsigneeNavigations { get; set; } = new List<DeliveryNoteHead>();

    public virtual ICollection<DeliveryNoteHead> DeliveryNoteHeadSalesmanNavigations { get; set; } = new List<DeliveryNoteHead>();

    public virtual ICollection<DeliveryNoteHead> DeliveryNoteHeadShipperNavigations { get; set; } = new List<DeliveryNoteHead>();
}
