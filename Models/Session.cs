using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Session
{
    public int Id { get; set; }

    public int? Tid { get; set; }

    [Required(ErrorMessage ="Select Branch.")]
    public int? Bid { get; set; }

    public string? Link { get; set; } = null!;

    [Required(ErrorMessage = "Session Detail is required.")]
    public string? Detail { get; set; }

    [Required(ErrorMessage = "Session Time is required.")]
    public string Time { get; set; } = null!;

    public virtual Branch? BidNavigation { get; set; }

    public virtual Coordinator? TidNavigation { get; set; }
}
