using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendAPI.Data;
using BackendAPI.Models;

[Route("api/[controller]")]
[ApiController]
public class UserProgressHistoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserProgressHistoryController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ GET: api/UserProgressHistory/5
    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<UserProgressHistory>>> GetUserProgress(int userId)
    {
        var progressList = await _context.UserProgressHistory
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        if (progressList.Count == 0)
            return NotFound("Không có dữ liệu tiến trình.");

        return Ok(progressList);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserProgressHistoryDto>>> GetAll()
    {
        var data = await _context.UserProgressHistory
            .Include(p => p.User)
            .Include(p => p.PTUser)
            .Include(p => p.Schedule)
                .ThenInclude(s => s.Class)     // lấy tên lớp
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new UserProgressHistoryDto
            {
                Id = p.Id,
                Notes = p.Note,
                CreatedAt = p.CreatedAt,

                // USER
                UserId = p.UserId,
                UserName = p.User.Name,

                // PT
                PTUserId = p.PTUserId,
                PTName = p.PTUser != null ? p.PTUser.Name : null,

                // SCHEDULE
                ScheduleId = p.ScheduleId,
                ScheduleName = p.Schedule != null
                    ? $"{p.Schedule.Class.ClassName} - {p.Schedule.StartTime:dd/MM HH:mm}"
                    : null,

                // Body data
                Weight = (double?)p.Weight,
                BodyFat = (double?)p.BodyFat,
                MuscleMass = (double?)p.MuscleMass,
                Height = (double?)p.Height,
                Waist = (double?)p.Waist,
                Hip = (double?)p.Hip,
                Chest = (double?)p.Chest
            })
            .ToListAsync();

        return Ok(data);
    }




    // ✅ POST: Thêm mới HOẶC sửa nếu TRÙNG NGÀY
    [HttpPost]
    public async Task<IActionResult> SaveProgress(UserProgressHistory model)
    {
        if (model.UserId == 0)
            return BadRequest("Thiếu UserId");

        // ✅ Lấy ngày không tính giờ
        var targetDate = model.CreatedAt.Date;

        var existing = await _context.UserProgressHistory.FirstOrDefaultAsync(p =>
            p.UserId == model.UserId &&
            p.CreatedAt.Date == targetDate
        );

        if (existing != null)
        {
            // ✅ UPDATE nếu đã tồn tại ngày đó
            existing.Weight = model.Weight;
            existing.BodyFat = model.BodyFat;
            existing.MuscleMass = model.MuscleMass;
            existing.Note = model.Note;
            existing.PTUserId = model.PTUserId;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công");
        }
        else
        {
            // ✅ INSERT nếu là ngày mới
            model.CreatedAt = DateTime.UtcNow;
            _context.UserProgressHistory.Add(model);
            await _context.SaveChangesAsync();
            return Ok("Thêm mới thành công");
        }
    }

    // PUT: api/UserProgressHistory/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProgress(int id, UserProgressHistory model)
    {
        if (id != model.Id)
            return BadRequest("ID mismatch");

        var existing = await _context.UserProgressHistory.FindAsync(id);
        if (existing == null)
            return NotFound("Không tìm thấy dữ liệu.");

        // Cập nhật các trường
        existing.Weight = model.Weight;
        existing.BodyFat = model.BodyFat;
        existing.MuscleMass = model.MuscleMass;
        existing.Note = model.Note;
        existing.PTUserId = model.PTUserId;
        existing.CreatedAt = model.CreatedAt;

        await _context.SaveChangesAsync();
        return Ok("Cập nhật thành công");
    }

    // ✅ DELETE: api/UserProgressHistory/3
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProgress(int id)
    {
        var progress = await _context.UserProgressHistory.FindAsync(id);
        if (progress == null)
            return NotFound("Không tìm thấy dữ liệu.");

        _context.UserProgressHistory.Remove(progress);
        await _context.SaveChangesAsync();

        return Ok("Xóa thành công");
    }
}
