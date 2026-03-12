using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Coordinator
{
    public int Tid { get; set; }

    [Required(ErrorMessage = "Select Branch.")]
    public int? Bid { get; set; }

    [Required(ErrorMessage = "Select Year.")]
    public int? Yid { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [RegularExpression(@"^([a-zA-Z]+\.?)\s([a-zA-Z]+\.?)\s([a-zA-Z]+\.?)$",
    ErrorMessage = "Name must be 3 parts. Only one dot is allowed at the end of each part (e.g., 'D. G. Tank').")]
    public string Tname { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Email is required.")]
    public string Contact { get; set; }

    [Required(ErrorMessage = "User Id is required.")]
    [StringLength(14, MinimumLength = 6, ErrorMessage = "User Id must be between 6 and 14 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,14}$",
    ErrorMessage = "User Id must have at least one uppercase, one lowercase, one digit, and one special character.")]
    public string Uid { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(14, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 14 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,14}$",
    ErrorMessage = "Password must have at least one uppercase, one lowercase, one digit, and one special character.")]
    public string Pwd { get; set; } = null!;

    public bool? Active { get; set; }

    public virtual Branch? BidNavigation { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual ICollection<Description> Descriptions { get; set; } = new List<Description>();

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public virtual ICollection<Rounddetail> Rounddetails { get; set; } = new List<Rounddetail>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<Studentsplaced> Studentsplaceds { get; set; } = new List<Studentsplaced>();

    public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();

    public virtual Year? YidNavigation { get; set; }
}
