namespace PlacementMentorshipPortal.Models
{
    public class DeactivateCoordinatorsViewModel
    {
        public int Tid { get; set; }
        public string Tname { get; set; }
        public string? BranchName { get; set; }
        public bool IsSelected { get; set; } // This binds to the checkbox
    }
}