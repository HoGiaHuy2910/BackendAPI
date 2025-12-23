using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/feedback
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetFeedbacks()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.PTUser)
                .Select(f => new
                {
                    f.Id,
                    UserName = f.User != null ? f.User.Name : "Unknown",
                    PTName = f.PTUser != null ? f.PTUser.Name : "N/A",
                    f.Comment,
                    f.Rating,
                    f.CreatedAt
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        // GET: api/feedback/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Feedback>> GetFeedback(int id)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.PTUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null)
                return NotFound(new { message = "Không tìm thấy feedback." });

            return Ok(feedback);
        }

        // POST: api/feedback
        [HttpPost]
        public async Task<ActionResult<Feedback>> CreateFeedback([FromBody] Feedback feedback)
        {
            if (feedback == null)
                return BadRequest();

            feedback.CreatedAt = DateTime.UtcNow;

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, feedback);
        }

        // PUT: api/feedback/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] Feedback updatedFeedback)
        {
            if (id != updatedFeedback.Id)
                return BadRequest();

            var existingFeedback = await _context.Feedbacks.FindAsync(id);
            if (existingFeedback == null)
                return NotFound();

            existingFeedback.Comment = updatedFeedback.Comment;
            existingFeedback.Rating = updatedFeedback.Rating;
            existingFeedback.PTUserId = updatedFeedback.PTUserId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/feedback/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("pt/{ptUserId}")]
        public async Task<IActionResult> GetPTFeedback(int ptUserId)
        {
            var data = await _context.Feedbacks
                .Where(f => f.PTUserId == ptUserId)
                .Select(f => new FeedbackDto
                {
                    FeedbackId = f.Id,
                    Comment = f.Comment,
                    Rating = f.Rating,
                    CreatedAt = f.CreatedAt,
                    UserName = f.User.Name
                })
                .ToListAsync();

            var ptInfo = await _context.Users
                .Where(u => u.Id == ptUserId)
                .Select(u => new PTInfoDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Specialty = _context.PTProfiles
                        .Where(p => p.UserId == u.Id)
                        .Select(p => p.Specialty)
                        .FirstOrDefault(),
                    ExperienceYears = _context.PTProfiles
                        .Where(p => p.UserId == u.Id)
                        .Select(p => p.ExperienceYears)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return Ok(new PTFeedbackResponse
            {
                PtInfo = ptInfo,
                Feedbacks = data
            });
        }

    }
}
