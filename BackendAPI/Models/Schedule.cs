namespace BackendAPI.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int? ClassId { get; set; }   // nullable cho 1-1
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public Class? Class { get; set; }   // navigation

        public ICollection<UserSchedules>? UserSchedules { get; set; }

    }
}
