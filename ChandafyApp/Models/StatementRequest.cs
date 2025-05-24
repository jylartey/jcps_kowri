using System.ComponentModel.DataAnnotations;

namespace ChandafyApp.Models
{
    public class StatementRequest
    {
        [Key]
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string RequestStatus { get; set; }
        public string RequestBy { get; set; } // e.g., Mail or Collector
        public DateTime IssueBy { get; set; }
        public string ApprovedBy { get; set; } // e.g., Mail or Collector
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation Property
        public Member Member { get; set; }
    }
}
