using RunnerBuddy.Helpers;
using System.Text.Json.Serialization;

namespace RunnerBuddy.Models.RecommendationPlan
{
    public class DaySuggestion
    {
        public string Day { get; set; }

        [JsonPropertyName("best_start_time")]
        public string BestStartTime { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("weather_description")]
        public string WeatherDescription { get; set; }
        public string WeatherIcon => WeatherIconMapper.GetIcon(WeatherDescription);

        public int Aqi { get; set; }
    }
}
