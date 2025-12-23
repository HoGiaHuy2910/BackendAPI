namespace BackendAPI.Models
{
    public class Class
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int? PTUserId { get; set; }

        public int? RoomId { get; set; }
        public int MaxParticipants { get; set; }
        public string Status { get; set; } = "Pending";

        public Room? Room { get; set; }
    }
}
