using System.Text.Json.Serialization;

namespace RunnerBuddy.Models.RecommendationPlan
{
    public class RunningPlan
    {
        public string Location { get; set; }

        [JsonPropertyName("overall_score")]
        public int OverallScore { get; set; }

        [JsonPropertyName("buddy_tip")]
        public string BuddyTip { get; set; }

        [JsonPropertyName("daily_suggestions")]
        public List<DaySuggestion> DailySuggestions { get; set; } = new List<DaySuggestion>();

        [JsonPropertyName("location_activities")]
        public List<string> LocationActivities { get; set; } = new List<string>();
    }
}
