using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models;

public partial class Student
{
    public int Id { get; set; }

    [EmailAddress]
    public string Mail { get; set; } = null!;

    public string Bname { get; set; } = null!;

    public int Entryyear { get; set; }
}
