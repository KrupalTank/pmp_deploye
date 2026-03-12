using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Description
{
    public int Id { get; set; }

    public int? Tid { get; set; }

    [Required(ErrorMessage = "Company is Required.")]
    public int? Cid { get; set; }

    [Required(ErrorMessage ="Description is Required.")]
    public string Dtext { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public virtual Company? CidNavigation { get; set; }

    public virtual Coordinator? TidNavigation { get; set; }
}
