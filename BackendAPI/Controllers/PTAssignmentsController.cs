using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PTAssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PTAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PTAssignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPTAssignments()
        {
            var result = await _context.PTAssignments
                .Include(p => p.PTUser)
                .Include(p => p.TraineeUser)
                .Select(p => new
                {
                    p.Id,
                    PTName = p.PTUser != null ? p.PTUser.Name : null,
                    TraineeName = p.TraineeUser != null ? p.TraineeUser.Name : null,
                    p.AssignedDate
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/PTAssignments/5
        [HttpGet("{traineeId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPTAssignment(int traineeId)
        {
            var pts = await _context.PTAssignments
                .Where(pa => pa.TraineeUserId == traineeId)
                .Select(pa => new
                {
                    PTId = pa.PTUser.Id,
                    PTName = pa.PTUser.Name,
                    PTEmail = pa.PTUser.Email
                })
                .ToListAsync();

            if (!pts.Any())
                return NotFound();

            return Ok(pts);
        }

        // ⭐⭐ API QUAN TRỌNG — MAUI CẦN API NÀY ⭐⭐
        // GET: api/PTAssignments/trainer/3/students
        [HttpGet("trainer/{ptUserId}/students")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentsByTrainer(int ptUserId)
        {
            var students = await _context.PTAssignments
                .Where(p => p.PTUserId == ptUserId)
                .Include(p => p.TraineeUser)
                .Select(p => new
                {
                    Id = p.TraineeUser.Id,
                    Name = p.TraineeUser.Name,
                    Email = p.TraineeUser.Email
                })
                .ToListAsync();

            return Ok(students);
        }

        // POST
        [HttpPost]
        public async Task<ActionResult<PTAssignment>> CreatePTAssignment(PTAssignment assignment)
        {
            var pt = await _context.Users.FindAsync(assignment.PTUserId);
            if (pt == null || !pt.Role.Equals("Trainer", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Người được gán PT phải có Role = 'Trainer'.");

            var trainee = await _context.Users.FindAsync(assignment.TraineeUserId);
            if (trainee == null)
                return BadRequest("Học viên không tồn tại.");

            bool exists = await _context.PTAssignments.AnyAsync(p =>
                p.PTUserId == assignment.PTUserId &&
                p.TraineeUserId == assignment.TraineeUserId);

            if (exists)
                return BadRequest("PT này đã được gán cho học viên này.");

            assignment.AssignedDate = DateTime.UtcNow;

            _context.PTAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPTAssignment), new { id = assignment.Id }, assignment);
        }

        // PUT: api/PTAssignments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePTAssignment(int id, PTAssignment assignment)
        {
            if (id != assignment.Id)
                return BadRequest();

            _context.Entry(assignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PTAssignments.Any(p => p.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePTAssignment(int id)
        {
            var assignment = await _context.PTAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            _context.PTAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
