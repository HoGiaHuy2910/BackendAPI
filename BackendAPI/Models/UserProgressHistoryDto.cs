namespace BackendAPI.Models
{
    public class UserProgressHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }

        public int? PTUserId { get; set; }
        public string? PTName { get; set; }

        public int? ScheduleId { get; set; }
        public string? ScheduleName { get; set; }

        public double? Weight { get; set; }
        public double? BodyFat { get; set; }
        public double? MuscleMass { get; set; }
        public double? Height { get; set; }
        public double? Waist { get; set; }
        public double? Hip { get; set; }
        public double? Chest { get; set; }

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
