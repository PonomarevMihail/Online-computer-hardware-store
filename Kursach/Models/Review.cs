using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models;

// Указываем точное имя таблицы и схему (store.reviews)
[Table("reviews", Schema = "store")]
public partial class Review
{
    [Key]
    [Column("number_of_review")] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int NumberOfReview { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Отзыв должен быть от 10 до 2000 символов.")]
    [Column("review")]
    public string Review1 { get; set; } = null!;

    [Range(1, 5)]
    [Column("rating")] 
    public int? Rating { get; set; }

    [Required]
    [Column("customer_id")] 
    public long CustomerId { get; set; }

    [Required]
    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")] 
    public DateTime? CreatedAt { get; set; }

    // --- Навигационные свойства (связи) ---
    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer Customer { get; set; } = null!;
}