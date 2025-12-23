using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("MembershipPlan")]
        public int PlanId { get; set; }   // 👈 KHÔNG đổi tên thành MembershipPlanId

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Quan hệ
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public MembershipPlan? MembershipPlan { get; set; }  // ⚡ EF sẽ dùng PlanId nhờ dòng trên
    }
}
