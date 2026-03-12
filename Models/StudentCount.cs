using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlacementMentorshipPortal.Models
{
    public partial class StudentCount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Yid { get; set; }

        [Required]
        public int Bid { get; set; }

        [Required(ErrorMessage = "Please enter the total number of eligible students.")]
        [Range(1, 1000, ErrorMessage = "Count must be a positive number.")]
        public int Count { get; set; }

        // Navigation properties for easy data access
        [ForeignKey("Yid")]
        public virtual Year? YearNavigation { get; set; }

        [ForeignKey("Bid")]
        public virtual Branch? BranchNavigation { get; set; }
    }
}