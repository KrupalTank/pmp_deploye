using System;
using System.Collections.Generic;

namespace PlacementMentorshipPortal.Models;

public partial class Company
{
    public int Cid { get; set; }

    public int? Tid { get; set; }

    public string Cname { get; set; } = null!;

    public string? Logo { get; set; }

    public virtual ICollection<Description> Descriptions { get; set; } = new List<Description>();

    public virtual ICollection<Rounddetail> Rounddetails { get; set; } = new List<Rounddetail>();

    public virtual ICollection<Studentsplaced> Studentsplaceds { get; set; } = new List<Studentsplaced>();

    public virtual Coordinator? TidNavigation { get; set; }
}
