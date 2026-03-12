namespace PlacementMentorshipPortal.Models
{
    using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class SendMailViewModel
    {
        //[Required]
        public int Tid { get; set; }

        [Required]
        public int Bid { get; set; }

        [Required]
        [StringLength(300)]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        [NotMapped]
        public List<IFormFile>? Attachments { get; set; }

        [ForeignKey("Bid")]
        public virtual Branch? BidNavigation { get; set; }
    }
}