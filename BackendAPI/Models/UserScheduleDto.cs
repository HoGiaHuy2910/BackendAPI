public class UserScheduleDto
{
    public int UserScheduleId { get; set; }   // 🔥 BẮT BUỘC


    public int ScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Thông tin phòng
    public string? RoomName { get; set; }

    // Thông tin PT (chỉ Trainer)
    public string? PTName { get; set; }

    // Optional: giữ thông tin Class nếu cần
    // public Class? Class { get; set; }
}
