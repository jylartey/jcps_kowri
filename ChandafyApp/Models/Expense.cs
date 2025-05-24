using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class Expense
{
    [Key]
    public int Id { get; set; }
    public string ExpenseType { get; set; }
    public decimal TotalExpectedAmount { get; set; }
    public string ExpenseDescription { get; set; }
    public string ExpenseReceiptImage { get; set; }
    public int FiscalYearId { get; set; }

    // Navigation Property
    public FiscalYear FiscalYear { get; set; }
}
