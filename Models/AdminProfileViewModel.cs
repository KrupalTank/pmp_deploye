namespace PlacementMentorshipPortal.Models
{
    public class AdminProfileViewModel
    {
        public List<Branch> branches { get; set; }
        public List<Year> years { get; set; }
        public List<Coordinator> coordinators { get; set; }

        public List<StudentCount> studentCounts { get; set; }

    }
}
