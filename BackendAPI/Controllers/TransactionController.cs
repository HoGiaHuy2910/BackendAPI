using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransaction()
        {
            return await _context.Transactions
                .Include(t => t.Payment)
                .ThenInclude(p => p.User)
                .Include(t => t.Payment)
                .ThenInclude(p => p.MembershipPlan)
                .ToListAsync();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Transaction>>> Get(
         [FromQuery] DateTime? from,
         [FromQuery] DateTime? to)
        {
            var query = from t in _context.Transactions
                        join p in _context.Payments on t.PaymentId equals p.Id
                        join u in _context.Users on p.UserId equals u.Id
                        join plan in _context.MembershipPlans on p.PlanId equals plan.Id
                        select new { t, p, u, plan };

            if (from.HasValue)
            {
                var f = from.Value.Date;
                query = query.Where(x => x.p.PaymentDate >= f);
            }

            if (to.HasValue)
            {
                var e = to.Value.Date.AddDays(1);
                query = query.Where(x => x.p.PaymentDate < e);
            }

            var data = await query
                .OrderByDescending(x => x.p.PaymentDate)
                .ThenByDescending(x => x.t.CreatedAt)
                .Select(x => new Transaction
                {
                    Id = x.t.Id,
                    PaymentId = x.p.Id,
                    UserName = x.u.Name,
                    PlanName = x.plan.PlanName,
                    Amount = x.p.Amount,
                    PaymentDate = x.p.PaymentDate,
                    TransactionType = x.t.TransactionType,
                    CreatedAt = x.t.CreatedAt
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
                return NotFound();

            return transaction;
        }

        // POST: api/transaction
        [HttpPost]
        public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] Transaction transaction)
        {
            if (transaction == null)
                return BadRequest();

            transaction.CreatedAt = DateTime.UtcNow;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }

        // PUT: api/transaction/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction updated)
        {
            if (id != updated.Id)
                return BadRequest();

            var existing = await _context.Transactions.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.TransactionType = updated.TransactionType;
            existing.Amount = updated.Amount;
            existing.PaymentId = updated.PaymentId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/transaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
