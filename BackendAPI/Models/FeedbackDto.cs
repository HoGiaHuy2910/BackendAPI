namespace BackendAPI.Models
{
    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
    }

    public class PTFeedbackResponse
    {
        public PTInfoDto PtInfo { get; set; }
        public List<FeedbackDto> Feedbacks { get; set; }
    }

    public class PTInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialty { get; set; }
        public int ExperienceYears { get; set; }
        public string Email { get; set; }
    }
}
