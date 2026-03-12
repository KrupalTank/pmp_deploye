using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlacementMentorshipPortal.Models
{
    public class Audit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Tid { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        public string Detail { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? Time { get; set; }

        // Navigation Property back to the Coordinator
        [ForeignKey("Tid")]
        public virtual Coordinator? TidNavigation { get; set; }
    }
}



