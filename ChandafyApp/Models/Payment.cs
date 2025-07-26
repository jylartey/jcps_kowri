using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models;
public class Payment
{
    [Key]
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int ChandaTypeId { get; set; }
    public int PaymentMethodId { get; set; }
    public DateTime PaymentDate { get; set; }
    public int FiscalYearId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionReference { get; set; }
    public string ReceiptNumber { get; set; }
    public bool Verified { get; set; }
    public string ApprovedBy { get; set; }
    public string Notes { get; set; }

    // Navigation Properties
    public Member Member { get; set; }
    public ChandaType ChandaType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public FiscalYear FiscalYear { get; internal set; }
}
