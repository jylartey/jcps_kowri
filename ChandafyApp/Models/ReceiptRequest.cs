using ChandafyApp.Data;
using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models
{
    public class ReceiptRequest
    {
        [Key]
        public int Id { get; set; }
        public string RequestStatus { get; set; }
        public string RequestById { get; set; } // Foreign key to Member (who requested)
        public DateTime IssueBy { get; set; }
        public string ApprovedById { get; set; } // Foreign key to Member (who approved)
        public int PaymentId { get; set; } // Foreign key to Payment
        public string ReceiptImage { get; set; }

        // Navigation Properties
        //public ApplicationUser RequestBy { get; set; }
        //public ApplicationUser ApprovedBy { get; set; }
        public Payment Payment { get; set; }
    }
}
