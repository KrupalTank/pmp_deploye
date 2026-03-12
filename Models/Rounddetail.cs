using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Rounddetail
{
    public int Id { get; set; }

    public int? Tid { get; set; }

    [Required(ErrorMessage = "Select Company.")]

    public int? Cid { get; set; }

    [Required(ErrorMessage ="Detail of rounds are required.")]
    public string Dtext { get; set; } = null!;

    public virtual Company? CidNavigation { get; set; }

    public virtual Coordinator? TidNavigation { get; set; }
}
