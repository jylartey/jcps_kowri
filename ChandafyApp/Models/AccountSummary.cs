using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class AccountSummary
{
    [Key]
    public int Id { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalExpectedBudget { get; set; }
    public decimal TotalExpectedAmount { get; set; }
    public int FiscalYearId { get; set; }
    public decimal BalanceLeft { get; set; }

    // Navigation Property
    public FiscalYear FiscalYear { get; set; }
}
