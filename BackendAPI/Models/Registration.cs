namespace BackendAPI.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ClassId { get; set; }
        public int? PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";

        // 🔗 Navigation
        public User? User { get; set; }
        public Class? Class { get; set; }
        public MembershipPlan? Plan { get; set; }
    }
}
