using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/equipment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEquipments()
        {
            var equipments = await _context.Equipments
                .Include(e => e.Room)
                .Select(e => new
                {
                    e.Id,
                    e.EquipmentName,
                    e.Quantity,
                    e.ConditionStatus,
                    RoomName = e.Room != null ? e.Room.RoomName : "Chưa gán phòng"
                })
                .ToListAsync();

            return Ok(equipments);
        }

        // GET: api/equipment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> GetEquipment(int id)
        {
            var equipment = await _context.Equipments
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
                return NotFound();

            return Ok(equipment);
        }

        // POST: api/equipment
        [HttpPost]
        public async Task<ActionResult<Equipment>> CreateEquipment([FromBody] Equipment equipment)
        {
            if (equipment == null)
                return BadRequest();

            if (string.IsNullOrEmpty(equipment.ConditionStatus))
                equipment.ConditionStatus = "Good";

            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipment), new { id = equipment.Id }, equipment);
        }

        // PUT: api/equipment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipment(int id, [FromBody] Equipment updatedEquipment)
        {
            if (id != updatedEquipment.Id)
                return BadRequest();

            var existing = await _context.Equipments.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.EquipmentName = updatedEquipment.EquipmentName;
            existing.Quantity = updatedEquipment.Quantity;
            existing.ConditionStatus = updatedEquipment.ConditionStatus;
            existing.RoomId = updatedEquipment.RoomId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/equipment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null)
                return NotFound();

            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
