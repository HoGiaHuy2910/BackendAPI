using System;
using System.Collections.Generic;

namespace BackendAPI.Models
{
    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class RevenueViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public List<DailyRevenueViewModel> DailyRevenues { get; set; }
            = new List<DailyRevenueViewModel>();
    }
}
