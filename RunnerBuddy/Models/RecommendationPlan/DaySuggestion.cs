using RunnerBuddy.Helpers;

namespace RunnerBuddy.Models.RecommendationPlan
{
    public class DaySuggestion
    {
        public string Day { get; set; }
        public string BestStartTime { get; set; }
        public string Reason { get; set; }
        public string WeatherDescription { get; set; }
        public string WeatherIcon => WeatherIconMapper.GetIcon(WeatherDescription);
        public int Aqi { get; set; }
    }
}
