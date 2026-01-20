using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

public partial class Customer
{
    [Key]
    public long CustomerId { get; set; }
    [Required]
    [StringLength(72, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$",
    ErrorMessage = "Пароль должен содержать заглавные, строчные буквы и цифры.")]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[А-Яа-яA-Za-z\-]+$",ErrorMessage = "Имя может содержать только буквы и дефис.")]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[А-Яа-яA-Za-z\-]*$",
    ErrorMessage = "Фамилия может содержать только буквы и дефис.")]
    public string Surname { get; set; } = null!;

    [StringLength(100)]
    [RegularExpression(@"^[А-Яа-яA-Za-z\-]*$",
    ErrorMessage = "Отчество может содержать только буквы и дефис.")]
    [DisplayFormat(ConvertEmptyStringToNull = true)]
    public string? Patronymic { get; set; }

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[A-Za-z0-9._-]+$",
    ErrorMessage = "Логин может содержать только латинские буквы, цифры и символы . _ -")]
    public string Login { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты.")]
    [StringLength(254)]
    public string Email { get; set; } = null!;
    
    [Required]
    [RegularExpression(@"^(?:\+7|8)\d{10}$", ErrorMessage = "Номер телефона должен быть в формате +7XXXXXXXXXX или 8XXXXXXXXXX.")]
    [StringLength(12)]
    public string Phone { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;
}
