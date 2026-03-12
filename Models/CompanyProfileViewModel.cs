namespace PlacementMentorshipPortal.Models
{
    public class CompanyProfileViewModel
    {
        public Company company { get; set; }
        public List<Studentsplaced> studentsplaced { get; set; }

        public List<Description> experience { get; set; }

        public Rounddetail rounddetail { get; set; }
    }
}
