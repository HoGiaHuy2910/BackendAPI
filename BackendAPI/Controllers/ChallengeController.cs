using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChallengeController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 GET: api/challenge
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Challenge>>> GetChallenges()
        {
            return await _context.Challenges.ToListAsync();
        }

        // 🔍 GET: api/challenge/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Challenge>> GetChallenge(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound();

            return Ok(challenge);
        }
        // GET: api/challenge/participants/1
        [HttpGet("participants/{challengeId}")]
        public async Task<IActionResult> GetParticipants(int challengeId)
        {
            var participants = await _context.ChallengeParticipants
                .Where(cp => cp.ChallengeId == challengeId)
                .Join(_context.Users,
                      cp => cp.UserId,
                      u => u.Id,
                      (cp, u) => new ChallengeParticipantDto
                      {
                          ParticipantId = cp.Id,
                          UserId = u.Id,
                          UserName = u.Name,
                          CurrentPoints = cp.CurrentPoints,
                          JoinedAt = cp.JoinedAt
                      })
                .ToListAsync();

            return Ok(participants);
        }


        // 🏆 GET: api/challenge/leaderboard/1
        [HttpGet("leaderboard/{challengeId}")]
        public async Task<IActionResult> GetLeaderboard(int challengeId)
        {
            var leaderboard = await _context.ChallengeParticipants
                .Where(cp => cp.ChallengeId == challengeId)
                .Join(_context.Users,
                      cp => cp.UserId,
                      u => u.Id,
                      (cp, u) => new
                      {
                          UserId = u.Id,
                          UserName = u.Name,
                          Points = cp.CurrentPoints,
                          Progress = cp.ProgressValue
                      })
                .OrderByDescending(x => x.Points)
                .ThenByDescending(x => x.Progress)
                .Take(10)
                .ToListAsync();

            return Ok(leaderboard);
        }


        // ➕ POST: api/challenge
        [HttpPost]
        public async Task<ActionResult<Challenge>> CreateChallenge(Challenge challenge)
        {
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChallenge), new { id = challenge.Id }, challenge);
        }

        // ✏️ PUT: api/challenge/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallenge(int id, Challenge updatedChallenge)
        {
            if (id != updatedChallenge.Id)
                return BadRequest();

            var existing = await _context.Challenges.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = updatedChallenge.Title;
            existing.Description = updatedChallenge.Description;
            existing.RewardPoints = updatedChallenge.RewardPoints;
            existing.StartDate = updatedChallenge.StartDate;
            existing.EndDate = updatedChallenge.EndDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ❌ DELETE: api/challenge/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallenge(int id)
        {
            var challenge = await _context.Challenges.FindAsync(id);
            if (challenge == null)
                return NotFound();

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ➕ POST: api/challenge/1/join?userId=5
        [HttpPost("{challengeId}/join")]
        public async Task<IActionResult> JoinChallenge(int challengeId, [FromQuery] int userId)
        {
            // 1️⃣ Check challenge tồn tại
            var challengeExists = await _context.Challenges.AnyAsync(c => c.Id == challengeId);
            if (!challengeExists)
                return NotFound("Challenge không tồn tại");

            // 2️⃣ Check user đã join chưa
            var existed = await _context.ChallengeParticipants
                .AnyAsync(cp => cp.ChallengeId == challengeId && cp.UserId == userId);

            if (existed)
                return BadRequest("User đã tham gia challenge này");

            // 3️⃣ Tạo participant
            var participant = new ChallengeParticipant
            {
                ChallengeId = challengeId,
                UserId = userId,
                CurrentPoints = 0,
                ProgressValue = 0,
                IsCompleted = false,
                JoinedAt = DateTime.UtcNow
            };

            _context.ChallengeParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok("Đã tham gia challenge");
        }


        // 📊 GET: api/challenge/1/progress?userId=5
        [HttpGet("{challengeId}/progress")]
        public async Task<IActionResult> GetProgress(int challengeId, [FromQuery] int userId)
        {
            var participant = await _context.ChallengeParticipants
                .Where(cp => cp.ChallengeId == challengeId && cp.UserId == userId)
                .Select(cp => new
                {
                    cp.CurrentPoints,
                    cp.ProgressValue,
                    cp.IsCompleted,
                    cp.JoinedAt,
                    cp.CompletedAt
                })
                .FirstOrDefaultAsync();

            if (participant == null)
                return NotFound("User chưa tham gia challenge");

            return Ok(participant);
        }

        // GET: api/challenge/{id}/is-joined?userId=5
        [HttpGet("{challengeId}/is-joined")]
        public async Task<IActionResult> IsJoined(int challengeId, int userId)
        {
            var joined = await _context.ChallengeParticipants
                .AnyAsync(x => x.ChallengeId == challengeId && x.UserId == userId);

            return Ok(joined);
        }


        // PUT: api/challenge/participant/5/progress
        [HttpPut("participant/{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] int points)
        {
            var participant = await _context.ChallengeParticipants.FindAsync(id);
            if (participant == null)
                return NotFound();

            participant.CurrentPoints = points;
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
