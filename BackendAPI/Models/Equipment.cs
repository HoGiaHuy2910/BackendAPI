namespace BackendAPI.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string ConditionStatus { get; set; } = "Good";

        // Quan hệ với phòng
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
    }
}
