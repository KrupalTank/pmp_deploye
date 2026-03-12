namespace PlacementMentorshipPortal.Models
{
    public class StudentStatsViewModel
    {
        public List<string> YearlyLabels { get; set; } = new List<string>();
        public List<int> YearlyCounts { get; set; } = new List<int>();

        public string SelectedBranchName { get; set; }
        public List<string> BranchYearlyLabels { get; set; } = new List<string>();
        public List<int> BranchYearlyCounts { get; set; } = new List<int>();
    }
}