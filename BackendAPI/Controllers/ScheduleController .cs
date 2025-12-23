using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScheduleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/schedule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
        {
            return await _context.Schedules
                .Include(s => s.Class)
                .ToListAsync();
        }

        // GET: api/schedule/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                return NotFound();

            return Ok(schedule);
        }

        // GET: api/schedule/calendar/user/5
        [HttpGet("calendar/user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserScheduleDto>>> GetUserCalendarSchedules(int userId)
        {
            // 1️⃣ Lấy tất cả UserSchedules của user, bao gồm Schedule → Class → Room
            var userSchedules = await _context.UserSchedules
                .Include(us => us.Schedule)
                    .ThenInclude(s => s.Class)
                        .ThenInclude(c => c.Room)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            // 2️⃣ Khởi tạo kết quả
            var result = new List<UserScheduleDto>();

            foreach (var us in userSchedules)
            {
                var s = us.Schedule;

                string ptName = null;

                if (s.Class != null && s.Class.PTUserId != null)
                {
                    // PT của lớp (Class)
                    ptName = await _context.Users
                        .Where(u => u.Id == s.Class.PTUserId && u.Role == "Trainer")
                        .Select(u => u.Name)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    // 1-1: lấy PT từ PTAssignments
                    ptName = await _context.PTAssignments
                        .Where(a => a.TraineeUserId == userId)
                        .Select(a => _context.Users
                                    .Where(u => u.Id == a.PTUserId && u.Role == "Trainer")
                                    .Select(u => u.Name)
                                    .FirstOrDefault())
                        .FirstOrDefaultAsync();
                }

                result.Add(new UserScheduleDto
                {
                    UserScheduleId = us.Id,        // 🔥 THÊM DÒNG NÀY

                    ScheduleId = s.Id,
                    ClassName = s.Class?.ClassName ?? "Gói tập 1-1",
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    RoomName = s.Class?.Room?.RoomName ?? "Phòng tập 1-1",
                    PTName = ptName
                });

            }

            return Ok(result);
        }




        [HttpPost]
        public async Task<IActionResult> CreateSchedule(
       [FromBody] Schedule schedule,
       [FromQuery] int? userId)
        {
            if (schedule == null)
                return BadRequest("Schedule trống");

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            if (userId.HasValue)
            {
                var userSchedule = new UserSchedules
                {
                    UserId = userId.Value,
                    ScheduleId = schedule.Id
                };

                _context.UserSchedules.Add(userSchedule);
                await _context.SaveChangesAsync();
            }

            // ❌ ĐỪNG return schedule
            // return CreatedAtAction(...)

            return Ok(new
            {
                message = "Tạo lịch thành công",
                scheduleId = schedule.Id
            });
        }


        // PUT: api/schedule/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule schedule)
        {
            if (id != schedule.Id)
                return BadRequest();

            var existing = await _context.Schedules.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.ClassId = schedule.ClassId;
            existing.StartTime = schedule.StartTime;
            existing.EndTime = schedule.EndTime;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/schedule/userschedule/5
        [HttpDelete("userschedule/{id}")]
        public async Task<IActionResult> DeleteUserSchedule(int id)
        {
            var us = await _context.UserSchedules
                .Include(x => x.Schedule)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (us == null)
                return NotFound();

            _context.UserSchedules.Remove(us);
            _context.Schedules.Remove(us.Schedule);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserScheduleDto>>> GetUserSchedules(int userId)
        {
            var schedules = await _context.UserSchedules
                .Include(us => us.Schedule)
                    .ThenInclude(s => s.Class)
                        .ThenInclude(c => c.Room)
                .Where(us => us.UserId == userId)
                .Select(us => new UserScheduleDto
                {
                    ScheduleId = us.ScheduleId,
                    ClassName = us.Schedule.Class != null ? us.Schedule.Class.ClassName : "Gói tập 1-1",
                    StartTime = us.Schedule.StartTime,
                    EndTime = us.Schedule.EndTime,
                    RoomName = us.Schedule.Class != null && us.Schedule.Class.Room != null
                ? us.Schedule.Class.Room.RoomName
                : "Phòng tập 1-1",
                    PTName = us.Schedule.Class != null
                ? _context.Users
                    .Where(u => u.Id == us.Schedule.Class.PTUserId && u.Role == "Trainer")
                    .Select(u => u.Name)
                    .FirstOrDefault()
                : _context.PTAssignments
                    .Where(a => a.TraineeUserId == us.UserId)
                    .Select(a => _context.Users
                                .Where(u => u.Id == a.PTUserId && u.Role == "Trainer")
                                .Select(u => u.Name)
                                .FirstOrDefault())
                    .FirstOrDefault()
                })

                .ToListAsync();

            return Ok(schedules);
        }


        [HttpGet("pt/{ptUserId}")]
        public async Task<ActionResult<IEnumerable<UserScheduleDto>>> GetPTSchedules(int ptUserId)
        {
            // Lấy tên PT (nếu cần)
            var ptName = await _context.Users
                .Where(u => u.Id == ptUserId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            // Lọc các Schedule liên quan tới các Class có PTUserId == ptUserId
            // Nếu model của bạn lưu PT id ở trường khác, thay PTUserId cho đúng.
            var schedules = await _context.Schedules
                .Include(s => s.Class)
                    .ThenInclude(c => c.Room)
                .Where(s => s.Class != null && s.Class.PTUserId == ptUserId)
                .Select(s => new UserScheduleDto
                {
                    ScheduleId = s.Id,
                    ClassName = s.Class != null ? s.Class.ClassName : string.Empty,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    RoomName = s.Class != null && s.Class.Room != null ? s.Class.Room.RoomName : null,
                    PTName = ptName
                })
                .ToListAsync();

            return Ok(schedules);
        }
    }
}