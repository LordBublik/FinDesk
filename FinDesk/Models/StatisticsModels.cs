namespace FinDesk.Models
{
    public class CategoryStatistic
    {
        public string CategoryName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
    }

    public class DailyStatistic
    {
        public string DateLabel { get; set; } = "";
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }
}