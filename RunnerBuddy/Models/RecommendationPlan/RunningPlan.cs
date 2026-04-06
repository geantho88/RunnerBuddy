namespace RunnerBuddy.Models.RecommendationPlan
{
    public class RunningPlan
    {
        public string Location { get; set; }
        public int OverallScore { get; set; }
        public string BuddyTip { get; set; }
        public List<DaySuggestion> DailySuggestions { get; set; } = new List<DaySuggestion>();
    }
}
