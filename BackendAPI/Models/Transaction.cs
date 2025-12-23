using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Payment")]
        public int? PaymentId { get; set; }   // có thể null vì SQL cho phép

        [NotMapped]
        public string UserName { get; set; }     // Tên user

        [NotMapped]
        public string PlanName { get; set; }     // Tên gói tập

        [MaxLength(50)]
        public string? TransactionType { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [NotMapped]
        public DateTime PaymentDate { get; set; } // Ngày thanh toán

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ
        public Payment? Payment { get; set; }
    }
}