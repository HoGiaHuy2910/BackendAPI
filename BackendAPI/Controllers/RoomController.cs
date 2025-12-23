using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase   // ⚡️ đổi thành RoomsController
{
    private readonly AppDbContext _context;

    public RoomsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
    {
        return await _context.Rooms.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return NotFound();
        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<Room>> CreateRoom([FromBody] Room room)
    {
        if (room == null)
            return BadRequest("Không có dữ liệu phòng.");

        // Kiểm tra bắt buộc
        if (string.IsNullOrWhiteSpace(room.RoomName))
            return BadRequest("Tên phòng không được để trống.");

        if (room.Capacity <= 0)
            return BadRequest("Sức chứa phải lớn hơn 0.");

        // Gán mặc định
        room.Status ??= "Available";

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        // Trả về 201 Created + dữ liệu mới
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] Room updatedRoom)
    {
        if (id != updatedRoom.Id)
            return BadRequest();

        var existingRoom = await _context.Rooms.FindAsync(id);
        if (existingRoom == null)
            return NotFound();

        existingRoom.RoomName = updatedRoom.RoomName;
        existingRoom.Capacity = updatedRoom.Capacity;
        existingRoom.Status = updatedRoom.Status ?? existingRoom.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return NotFound();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
