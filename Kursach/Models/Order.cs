using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursachBD.Models
{
    
    public class PaymentDateNotFutureAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime paymentDate)
                return ValidationResult.Success;

            if (paymentDate > DateTime.Now)
                return new ValidationResult("Дата оплаты не может быть позже сегодняшней.");

            return ValidationResult.Success;
        }
    }

    
    public class DeliveryDateRangeAttribute : ValidationAttribute
    {
        private readonly int _maxDays;

        public DeliveryDateRangeAttribute(int maxDays)
        {
            _maxDays = maxDays;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime deliveryDate)
                return ValidationResult.Success;

            var order = (Order)validationContext.ObjectInstance;

            // Если заказ не оплачен — не сравниваем доставку с датой оплаты
            if (!order.PaymentDate.HasValue)
                return ValidationResult.Success;

            var payDate = order.PaymentDate.Value.Date;

            if (deliveryDate.Date < payDate)
                return new ValidationResult("Дата доставки не может быть раньше даты оплаты.");

            if (deliveryDate.Date > payDate.AddDays(_maxDays))
                return new ValidationResult($"Срок доставки не может превышать {_maxDays} дней после оплаты.");

            return ValidationResult.Success;
        }
    }

    public partial class Order
    {
        [Key]
        public long NumberOfOrder { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = null!;

        [StringLength(1000)]
        public string? Comment { get; set; }

        [Required]
        [ForeignKey(nameof(Customer))]
        public long CustomerId { get; set; }

        [Required]
        [Column(TypeName = "timestamp without time zone")]
        [DeliveryDateRange(14)]
        public DateTime DeliveryDate { get; set; }

        [Column("payment_date", TypeName = "timestamp without time zone")]
        [PaymentDateNotFuture]
        public DateTime? PaymentDate { get; set; }
        public virtual ICollection<ContentOfOrder> ContentOfOrders { get; set; } = new List<ContentOfOrder>();
    }
}
