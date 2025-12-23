using BackendAPI.Data;      // DbContext AppDbContext
using BackendAPI.Models;    // RevenueViewModel, DailyRevenueViewModel
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RevenueController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/Revenue
        [HttpGet]
        public async Task<ActionResult<RevenueViewModel>> GetAll()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var nextMonth = firstDayOfMonth.AddMonths(1);

            var payments = await _context.Payments.ToListAsync();

            var model = new RevenueViewModel
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                RevenueToday = payments
                    .Where(p => p.PaymentDate.Date == today)
                    .Sum(p => p.Amount),
                RevenueThisMonth = payments
                    .Where(p => p.PaymentDate >= firstDayOfMonth &&
                                p.PaymentDate < nextMonth)
                    .Sum(p => p.Amount),
                DailyRevenues = payments
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new DailyRevenueViewModel
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Amount = g.Sum(x => x.Amount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList()
            };

            return Ok(model);
        }

        // GET: /api/Revenue/filter?start=2025-11-01&end=2025-11-30
        [HttpGet("filter")]
        public async Task<ActionResult<RevenueViewModel>> GetByRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            var startDate = start.Date;
            var endExclusive = end.Date.AddDays(1);

            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= startDate &&
                            p.PaymentDate < endExclusive)
                .ToListAsync();

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var nextMonth = firstDayOfMonth.AddMonths(1);

            var model = new RevenueViewModel
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                RevenueToday = payments
                    .Where(p => p.PaymentDate.Date == today)
                    .Sum(p => p.Amount),
                RevenueThisMonth = payments
                    .Where(p => p.PaymentDate >= firstDayOfMonth &&
                                p.PaymentDate < nextMonth)
                    .Sum(p => p.Amount),
                DailyRevenues = payments
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new DailyRevenueViewModel
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Amount = g.Sum(x => x.Amount)
                    })
                    .OrderBy(x => x.Date)
                    .ToList()
            };

            return Ok(model);
        }
    }
}
