using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipPlanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembershipPlanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/membershipplan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetPlans()
        {
            var plans = await _context.MembershipPlans
                .OrderBy(p => p.Price)
                .ToListAsync();
            return Ok(plans);
        }

        // GET: api/membershipplan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipPlan>> GetPlan(int id)
        {
            var plan = await _context.MembershipPlans.FindAsync(id);
            if (plan == null)
                return NotFound(new { message = $"Không tìm thấy gói tập có ID = {id}" });

            return Ok(plan);
        }

        // POST: api/membershipplan
        [HttpPost]
        public async Task<ActionResult<MembershipPlan>> CreatePlan([FromBody] MembershipPlan plan)
        {
            if (plan == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            plan.CreatedAt = DateTime.UtcNow;
            _context.MembershipPlans.Add(plan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
        }

        // PUT: api/membershipplan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] MembershipPlan updatedPlan)
        {
            if (id != updatedPlan.Id)
                return BadRequest(new { message = "ID không khớp" });

            var existing = await _context.MembershipPlans.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy gói tập" });

            existing.PlanName = updatedPlan.PlanName;
            existing.DurationMonths = updatedPlan.DurationMonths;
            existing.Price = updatedPlan.Price;
            existing.Description = updatedPlan.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // DELETE: api/membershipplan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.MembershipPlans.FindAsync(id);
            if (plan == null)
                return NotFound(new { message = "Không tìm thấy gói tập" });

            _context.MembershipPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
