using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models
{
    public class ReceiptRequest
    {
        [Key]
        public int Id { get; set; }
        public string RequestStatus { get; set; }
        public int RequestById { get; set; } // Foreign key to Member (who requested)
        public DateTime IssueBy { get; set; }
        public int ApprovedById { get; set; } // Foreign key to Member (who approved)
        public int PaymentId { get; set; } // Foreign key to Payment
        public string ReceiptImage { get; set; }

        // Navigation Properties
        public Member RequestBy { get; set; }
        public Member ApprovedBy { get; set; }
        public Payment Payment { get; set; }
    }
}
