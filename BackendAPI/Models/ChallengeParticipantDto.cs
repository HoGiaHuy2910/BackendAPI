namespace BackendAPI.Models
{
    public class ChallengeParticipantDto
    {
        public int ParticipantId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int CurrentPoints { get; set; }
        public DateTime JoinedAt { get; set; }
    }

}
