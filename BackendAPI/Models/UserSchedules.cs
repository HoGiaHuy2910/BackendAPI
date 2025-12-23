namespace BackendAPI.Models
{
    public class UserSchedules
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ScheduleId { get; set; }

        public User? User { get; set; }
        public Schedule? Schedule { get; set; }
    }
}
