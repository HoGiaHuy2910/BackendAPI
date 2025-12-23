namespace BackendAPI.Models
{
    public class PTAssignment
    {
        public int Id { get; set; }
        public int PTUserId { get; set; }
        public int TraineeUserId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        // 🔗 Navigation
        public User? PTUser { get; set; }
        public User? TraineeUser { get; set; }
    }
}
