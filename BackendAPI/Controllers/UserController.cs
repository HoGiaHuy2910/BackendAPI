using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendAPI.Data;
using BackendAPI.Models;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        // GET: api/user/statistics
        [HttpGet("statistics")]
        public async Task<ActionResult> GetUserStatistics()
        {
            var total = await _context.Users.CountAsync();
            var admins = await _context.Users.CountAsync(u => u.Role == "Admin");
            var trainers = await _context.Users.CountAsync(u => u.Role == "Trainer");
            var customers = await _context.Users.CountAsync(u => u.Role == "Customer");

            return Ok(new
            {
                TotalUsers = total,
                Admins = admins,
                Trainers = trainers,
                Customers = customers
            });
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.Password = updatedUser.Password;
            existingUser.Role = updatedUser.Role;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/registration/user/{userId}/plans
        [HttpGet("user/{userId}/plans")]
        public async Task<ActionResult<List<MembershipPlan>>> GetUserPlans(int userId)
        {
            var plans = await _context.Registrations
                .Include(r => r.Plan)
                .Where(r => r.UserId == userId && r.PlanId != null)
                .Select(r => r.Plan!)
                .ToListAsync();

            if (plans.Count == 0)
                return NotFound();

            return Ok(plans);
        }

        // GET: api/registration/user/{userId}/classes
        [HttpGet("user/{userId}/classes")]
        public async Task<ActionResult<List<Class>>> GetUserClasses(int userId)
        {
            var classes = await _context.Registrations
                .Include(r => r.Class)
                    .ThenInclude(c => c.Room)
                .Where(r => r.UserId == userId && r.ClassId != null)
                .Select(r => r.Class!)
                .ToListAsync();

            if (classes.Count == 0)
                return NotFound();

            return Ok(classes);
        }
    }
}
