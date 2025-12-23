using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendAPI.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        // FK -> dbo.UserSchedules(Id)
        [ForeignKey(nameof(UserSchedule))]
        public int UserScheduleId { get; set; }

        // maps to SQL DATE
        [Column(TypeName = "date")]
        public DateTime AttendanceDate { get; set; } = DateTime.UtcNow.Date;

        public bool IsPresent { get; set; } = true;

        // Navigation: UserSchedules contains UserId and ScheduleId
        public UserSchedules? UserSchedule { get; set; }
    }
}