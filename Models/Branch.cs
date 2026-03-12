using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Branch
{
    public int Bid { get; set; }

    [Required(ErrorMessage = "Enter Branch Name.")]
    public string Bname { get; set; } = null!;

    public virtual ICollection<Coordinator> Coordinators { get; set; } = new List<Coordinator>();

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<Studentsplaced> Studentsplaceds { get; set; } = new List<Studentsplaced>();

    public virtual ICollection<StudentCount> StudentCounts { get; set; } = new List<StudentCount>();
}
