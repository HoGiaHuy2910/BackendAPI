namespace BackendAPI.Models
{
    public class ChallengeParticipant
    {
        public int Id { get; set; }
        public int ChallengeId { get; set; }
        public int UserId { get; set; }
        public int CurrentPoints { get; set; }
        public double ProgressValue { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        public string Status { get; set; } = "Active";

        public DateTime JoinedAt { get; set; }
        public Challenge? Challenge { get; set; }
        public User? User { get; set; }
    }

}
