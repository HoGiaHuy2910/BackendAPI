
namespace BackendAPI.Controllers
{
    internal class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}