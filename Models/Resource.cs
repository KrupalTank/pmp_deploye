using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Resource
{
    public int Id { get; set; }

    public int? Tid { get; set; }

    [Required(ErrorMessage ="Select Branch.")]
    public int? Bid { get; set; }

    [Required(ErrorMessage = "Link is Required.")]
    public string Rlink { get; set; } = null!;

    [Required(ErrorMessage = "Detail is Required.")]
    public string Details { get; set; } = null!;

    public virtual Branch? BidNavigation { get; set; }

    public virtual Coordinator? TidNavigation { get; set; }
}
