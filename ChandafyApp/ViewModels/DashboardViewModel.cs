using ChandafyApp.Models;

namespace ChandafyApp.NewFolder
{
    public class DashboardViewModel
    {
        public AccountSummary AccountSummary { get; set; }
        public FiscalYear CurrentFiscalYear { get; set; }
        public List<FiscalYear> FiscalYears { get; set; }
        public List<Payment> RecentPayments { get; set; }
        public Dictionary<string, decimal> ChandaTypePayments { get; set; }
        public List<decimal> MonthlyBudget { get; set; }
        public List<decimal> MonthlyPayments { get; set; }
    }

    public class AccountSummary
    {
        public decimal TotalPayments { get; set; }
        public decimal TotalExpectedBudget { get; set; }
        public decimal BalanceLeft { get; set; }
        public decimal PercentageComplete { get; set; }
    }
    //public class Payment
    //{
    //    public int Id { get; set; }
    //    public DateTime PaymentDate { get; set; }
    //    public decimal Amount { get; set; }
    //    public string ReceiptNumber { get; set; }
    //    public bool Verified { get; set; }
    //    public ChandaType ChandaType { get; set; }
    //}

    //public class ChandaType
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    //public class FiscalYear
    //{
    //    public int Id { get; set; }
    //    public int Year { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //}
}
