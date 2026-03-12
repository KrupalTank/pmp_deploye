using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models
{
    public class CompanyWithImage
    {
        public int Cid { get; set; }

        public int? Tid { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        public string Cname { get; set; } = null!;

        public IFormFile LogoPath { get; set; }

        public string? Logo {  get; set; }= null!;
    }
}
