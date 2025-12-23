using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/attendance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendances()
        {
            var list = await _context.Attendances
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.User)
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.Schedule)
                        .ThenInclude(s => s.Class)
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/attendance/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Attendance>> GetAttendance(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.User)
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.Schedule)
                        .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
                return NotFound();

            return Ok(attendance);
        }

        // GET: api/attendance/userschedule/5
        [HttpGet("userschedule/{userScheduleId}")]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetByUserSchedule(int userScheduleId)
        {
            var list = await _context.Attendances
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.User)
                .Where(a => a.UserScheduleId == userScheduleId)
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/attendance/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetByUser(int userId)
        {
            var list = await _context.Attendances
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.Schedule)
                        .ThenInclude(s => s.Class)
                .Include(a => a.UserSchedule)
                    .ThenInclude(us => us.User)
                .Where(a => a.UserSchedule != null && a.UserSchedule.UserId == userId)
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/attendance
        [HttpPost]
        public async Task<ActionResult<Attendance>> CreateAttendance([FromBody] Attendance attendance)
        {
            var usExists = await _context.UserSchedules.AnyAsync(us => us.Id == attendance.UserScheduleId);
            if (!usExists)
                return BadRequest("Invalid UserScheduleId.");

            if (attendance.AttendanceDate == default)
                attendance.AttendanceDate = DateTime.UtcNow.Date;

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, attendance);
        }

        // PUT: api/attendance/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] Attendance attendance)
        {
            if (id != attendance.Id)
                return BadRequest();

            var existing = await _context.Attendances.FindAsync(id);
            if (existing == null)
                return NotFound();

            if (attendance.UserScheduleId != existing.UserScheduleId)
            {
                var usExists = await _context.UserSchedules.AnyAsync(us => us.Id == attendance.UserScheduleId);
                if (!usExists)
                    return BadRequest("Invalid UserScheduleId.");

                existing.UserScheduleId = attendance.UserScheduleId;
            }

            existing.AttendanceDate = attendance.AttendanceDate;
            existing.IsPresent = attendance.IsPresent;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/attendance/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return NotFound();

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------ New endpoints for PT check-in workflow ------------------

        // GET: api/attendance/schedule/{scheduleId}/participants?date=2025-11-13
        // Trả về danh sách User (đã đăng ký vào Schedule qua UserSchedules) và trạng thái IsPresent cho ngày đó.
        [HttpGet("schedule/{scheduleId}/participants")]
        public async Task<ActionResult<IEnumerable<ParticipantDto>>> GetParticipantsForSchedule(int scheduleId, [FromQuery] DateTime? date)
        {
            var targetDate = (date ?? DateTime.UtcNow).Date;

            // Lấy tất cả userSchedules cho schedule này, include User
            var userSchedules = await _context.UserSchedules
                .Include(us => us.User)
                .Where(us => us.ScheduleId == scheduleId)
                .ToListAsync();

            if (!userSchedules.Any())
                return Ok(new List<ParticipantDto>());

            var userScheduleIds = userSchedules.Select(us => us.Id).ToList();

            // Lấy attendance đã có cho ngày này trên những userSchedule này
            var attendances = await _context.Attendances
                .Where(a => userScheduleIds.Contains(a.UserScheduleId) && a.AttendanceDate == targetDate)
                .ToListAsync();

            var participants = userSchedules.Select(us =>
            {
                var att = attendances.FirstOrDefault(a => a.UserScheduleId == us.Id);
                return new ParticipantDto
                {
                    UserScheduleId = us.Id,
                    UserId = us.UserId,
                    Name = us.User?.Name ?? string.Empty,
                    IsPresent = att != null ? att.IsPresent : false
                };
            }).ToList();

            return Ok(participants);
        }

        // POST: api/attendance/mark
        // Payload:
        // {
        //   "ScheduleId": 123,
        //   "AttendanceDate": "2025-11-13", // optional
        //   "Attendees": [ { "UserId": 1, "Present": true }, ... ]
        // }
        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request)
        {
            if (request == null || request.Attendees == null || !request.Attendees.Any())
                return BadRequest("Invalid payload.");

            var date = (request.AttendanceDate ?? DateTime.UtcNow).Date;

            var userIds = request.Attendees.Select(a => a.UserId).Distinct().ToList();

            var userSchedules = await _context.UserSchedules
                .Where(us => us.ScheduleId == request.ScheduleId && userIds.Contains(us.UserId))
                .ToListAsync();

            if (!userSchedules.Any())
                return BadRequest("No participants found for the given ScheduleId and UserIds.");

            var usIds = userSchedules.Select(us => us.Id).ToList();
            var existingAttendances = await _context.Attendances
                .Where(a => usIds.Contains(a.UserScheduleId) && a.AttendanceDate == date)
                .ToListAsync();

            foreach (var attendee in request.Attendees)
            {
                var us = userSchedules.FirstOrDefault(x => x.UserId == attendee.UserId);
                if (us == null)
                    continue;

                var existing = existingAttendances.FirstOrDefault(a => a.UserScheduleId == us.Id && a.AttendanceDate == date);
                if (existing != null)
                {
                    existing.IsPresent = attendee.Present;
                }
                else
                {
                    var newAtt = new Attendance
                    {
                        UserScheduleId = us.Id,
                        AttendanceDate = date,
                        IsPresent = attendee.Present
                    };
                    _context.Attendances.Add(newAtt);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Attendance saved." });
        }
    }

    // DTOs used by the endpoints (these are not DB entities)
    public class ParticipantDto
    {
        public int UserScheduleId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPresent { get; set; } = false;
    }

    public class AttendeeDto
    {
        public int UserId { get; set; }
        public bool Present { get; set; }
    }

    public class MarkAttendanceRequest
    {
        public int ScheduleId { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public List<AttendeeDto>? Attendees { get; set; }
    }
}