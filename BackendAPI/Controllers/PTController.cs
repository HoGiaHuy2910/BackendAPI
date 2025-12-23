using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PTController : ControllerBase
{
    private readonly AppDbContext _context;

    public PTController(AppDbContext context)
    {
        _context = context;
    }

    // 📋 Lấy danh sách tất cả PT
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPTs()
    {
        var pts = await _context.PTProfiles
            .Join(
                _context.Users,
                pt => pt.UserId,
                user => user.Id,
                (pt, user) => new
                {
                    pt.Id,
                    pt.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    pt.Specialty,
                    pt.ExperienceYears
                }
            )
            .ToListAsync();


        return Ok(pts);
    }

    // 🔍 Lấy 1 PT theo id
    [HttpGet("{id}")]
    public async Task<ActionResult> GetPT(int id)
    {
        var pt = await _context.PTProfiles
            .Join(
                _context.Users,
                pt => pt.UserId,
                user => user.Id,
                (pt, user) => new
                {
                    pt.Id,
                    pt.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    pt.Specialty,
                    pt.ExperienceYears
                }
            )
            .FirstOrDefaultAsync(pt => pt.Id == id);

        if (pt == null)
            return NotFound(new { message = $"Không tìm thấy PT có id = {id}" });

        return Ok(pt);
    }

    // ✏️ Cập nhật PT (chỉ update PT, không đụng đến User)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePT(int id, [FromBody] PTProfile ptUpdate)
    {
        if (id != ptUpdate.Id)
            return BadRequest(new { message = "Id không khớp." });

        var existingPT = await _context.PTProfiles.FindAsync(id);
        if (existingPT == null)
            return NotFound(new { message = "Không tìm thấy PT." });

        existingPT.Specialty = ptUpdate.Specialty;
        existingPT.ExperienceYears = ptUpdate.ExperienceYears;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thành công!" });
    }

    // ❌ Xóa PT
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePT(int id)
    {
        var pt = await _context.PTProfiles.FindAsync(id);
        if (pt == null)
            return NotFound(new { message = "Không tìm thấy PT." });

        _context.PTProfiles.Remove(pt);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ✅ GET: PT mà user đang học qua lớp
    [HttpGet("my-classes-pts/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserClassPTs(int userId)
    {
        var result = await _context.Registrations
            .Where(r => r.UserId == userId && r.ClassId != null)
            .Join(_context.Classes,
                  r => r.ClassId,
                  c => c.Id,
                  (r, c) => c.PTUserId)
            .Distinct()
            .Join(_context.Users,
                  ptId => ptId,
                  u => u.Id,
                  (ptId, u) => new
                  {
                      PTId = u.Id,
                      PTName = u.Name,
                      u.Email,
                      Role = u.Role
                  })
            .Where(u => u.Role == "Trainer")
            .ToListAsync();

        return Ok(result);
    }
}
