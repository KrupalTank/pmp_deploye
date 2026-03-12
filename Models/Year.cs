using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Year
{
    public int Yid { get; set; }

    [Required(ErrorMessage = "Year is required.")]
    [YearRange] // Our custom dynamic range
    [RegularExpression(@"^\d+$", ErrorMessage = "Year must contain only digits.")]
    public int Year1 { get; set; }

    public virtual ICollection<Coordinator> Coordinators { get; set; } = new List<Coordinator>();

    public virtual ICollection<Studentsplaced> Studentsplaceds { get; set; } = new List<Studentsplaced>();

    public virtual ICollection<StudentCount> StudentCounts { get; set; } = new List<StudentCount>();
}
