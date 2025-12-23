using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserScheduleController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Lấy danh sách user schedule (kèm user + class)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUserSchedules()
        {
            var result = await _context.UserSchedules
                .Include(us => us.User)
                .Include(us => us.Schedule)
                    .ThenInclude(s => s.Class)
                .Select(us => new
                {
                    us.Id,
                    UserName = us.User != null ? us.User.Name : null,
                    ClassName = us.Schedule != null ? us.Schedule.Class.ClassName : null,
                    StartTime = us.Schedule != null ? us.Schedule.StartTime : DateTime.MinValue,
                    EndTime = us.Schedule != null ? us.Schedule.EndTime : DateTime.MinValue
                })
                .ToListAsync();

            return Ok(result);
        }


        // ✅ Lấy chi tiết gộp từ UserSchedules + Schedules + Classes + User
        // Trả về: id, UserId, UserName, UserEmail, ClassId, ClassName, StartTime, EndTime, PTUserId, RoomId, MaxParticipants
        [HttpGet("detailed")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserSchedulesDetailed()
        {
            var result = await _context.UserSchedules
                .Include(us => us.User)
                .Include(us => us.Schedule)
                    .ThenInclude(s => s.Class)
                .Select(us => new
                {
                    Id = us.Id,
                    UserId = us.UserId,
                    UserName = us.User != null ? us.User.Name : null,
                    UserEmail = us.User != null ? us.User.Email : null,

                    ScheduleId = us.ScheduleId,

                    ClassId = us.Schedule != null ? (int?)us.Schedule.ClassId : null,
                    ClassName = us.Schedule != null && us.Schedule.Class != null ? us.Schedule.Class.ClassName : null,

                    StartTime = us.Schedule != null ? (DateTime?)us.Schedule.StartTime : null,
                    EndTime = us.Schedule != null ? (DateTime?)us.Schedule.EndTime : null,

                    PTUserId = us.Schedule != null && us.Schedule.Class != null ? us.Schedule.Class.PTUserId : null,
                    RoomId = us.Schedule != null && us.Schedule.Class != null ? us.Schedule.Class.RoomId : null,
                    MaxParticipants = us.Schedule != null && us.Schedule.Class != null ? (int?)us.Schedule.Class.MaxParticipants : null
                })
                .ToListAsync();

            return Ok(result);
        }

        // ✅ Thêm mới
        [HttpPost]
        public async Task<ActionResult<UserSchedules>> CreateUserSchedule([FromBody] UserSchedules userSchedules)
        {
            _context.UserSchedules.Add(userSchedules);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserSchedules), new { id = userSchedules.Id }, userSchedules);
        }

        // ✅ Xóa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserSchedule(int id)
        {
            var item = await _context.UserSchedules.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.UserSchedules.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
