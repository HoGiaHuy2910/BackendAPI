using BackendAPI.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserProgressHistory")]
public class UserProgressHistory
{
    public int Id { get; set; }

    // --- USER (người tập) ---
    public int UserId { get; set; }
    public User User { get; set; }

    // --- SCHEDULE (lịch tập) ---
    public int? ScheduleId { get; set; }
    public Schedule? Schedule { get; set; }

    // --- TRAINER (PT) ---
    public int? PTUserId { get; set; }
    [ForeignKey("PTUserId")]
    public User? PTUser { get; set; }

    // --- BODY METRICS ---
    public decimal? Weight { get; set; }
    public decimal? BodyFat { get; set; }
    public decimal? MuscleMass { get; set; }
    public decimal? Height { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Hip { get; set; }
    public decimal? Chest { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
