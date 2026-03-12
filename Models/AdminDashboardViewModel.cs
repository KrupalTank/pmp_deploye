namespace PlacementMentorshipPortal.Models
{
    public class AdminDashboardViewModel
    {
        // Existing properties...
        public List<string> YearLabels { get; set; } = new List<string>();
        public List<int> YearData { get; set; } = new List<int>();
        public List<string> BranchLabels { get; set; } = new List<string>();
        public List<int> BranchData { get; set; } = new List<int>();
        public int CurrentYear { get; set; }
        public List<string> CurrentYearBranchLabels { get; set; } = new List<string>();
        public List<int> CurrentYearBranchData { get; set; } = new List<int>();

        // NEW: Package Statistics Properties
        public List<string> PackageBranchLabels { get; set; } = new List<string>();
        public List<double> AveragePackages { get; set; } = new List<double>();
        public List<double> HighestPackages { get; set; } = new List<double>();

        public List<string> CompanyLabels { get; set; } = new List<string>();
        public List<int> CompanyPlacementData { get; set; } = new List<int>();

        public List<Audit> RecentActivities { get; set; } = new List<Audit>();

        public List<StudentCount> StudentCounts { get; set; } = new List<StudentCount>();

        public List<double> YearPercentages { get; set; } = new List<double>();
        public List<double> BranchPercentages { get; set; } = new List<double>();
        public List<double> CurrentYearBranchPercentages { get; set; } = new List<double>();
    }
}
