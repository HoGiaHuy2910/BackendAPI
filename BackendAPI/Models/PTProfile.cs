namespace BackendAPI.Models
{
    public class PTProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
