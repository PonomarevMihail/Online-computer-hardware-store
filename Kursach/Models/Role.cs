using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

public partial class Role
{
    [Required]
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должности должно содержать от 2 до 100 символов.")]
    public string JobTitle { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
