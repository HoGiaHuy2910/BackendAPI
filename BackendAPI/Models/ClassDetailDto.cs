public class ClassDetailDto
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }

    // PT info
    public int? PTUserId { get; set; }
    public string? PTName { get; set; }
    public string? Specialty { get; set; }
    public int? ExperienceYears { get; set; }

    // Room info
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public int? Capacity { get; set; }
}
