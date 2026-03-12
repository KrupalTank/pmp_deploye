using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Studentsplaced
{
    public int Id { get; set; }

    public int? Tid { get; set; }

    [Required(ErrorMessage ="Select Branch.")]
    public int? Bid { get; set; }

    [Required(ErrorMessage ="Select Year.")]
    public int? Yid { get; set; }

    [Required(ErrorMessage ="Select Company.")]
    public int? Cid { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [RegularExpression(@"^([a-zA-Z]+\.?)\s([a-zA-Z]+\.?)\s([a-zA-Z]+\.?)$",
    ErrorMessage = "Name must be 3 parts. Only one dot is allowed at the end of each part (e.g., 'D. G. Tank').")]
    public string Sname { get; set; } = null!;

    [Required(ErrorMessage ="Add Package.")]
    public decimal? Package { get; set; }

    [Required(ErrorMessage = "Add Email.")]
    [EmailAddress(ErrorMessage = "Email address is required.")]
    public string? Contact { get; set; }

    public virtual Branch? BidNavigation { get; set; }

    public virtual Company? CidNavigation { get; set; }

    public virtual Coordinator? TidNavigation { get; set; }

    public virtual Year? YidNavigation { get; set; }
}
