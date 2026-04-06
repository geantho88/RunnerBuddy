using System;

namespace RunnerBuddy.Helpers
{
    public static class WeatherIconMapper
    {
        public static string GetIcon(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return "sunny.png"; // Default fallback

            var text = description.ToLowerInvariant();

            // 1. Thunderstorms (High priority)
            if (text.Contains("thunder") || text.Contains("storm"))
                return "thunderstorms.png";

            // 2. Snow Logic
            if (text.Contains("snow") || text.Contains("flurries"))
            {
                if (text.Contains("heavy") || text.Contains("blizzard")) return "snow_heavy.png";
                if (text.Contains("light") || text.Contains("patchy")) return "snow_light.png";
                if (text.Contains("cloud")) return "snow_s_cloudy.png";
                if (text.Contains("rain")) return "rain_s_snow.png";
                return "snow.png";
            }

            // 3. Rain Logic
            if (text.Contains("rain") || text.Contains("shower") || text.Contains("drizzle"))
            {
                if (text.Contains("heavy") || text.Contains("extreme")) return "rain_heavy.png";
                if (text.Contains("light") || text.Contains("patchy")) return "rain_light.png";
                if (text.Contains("sun")) return "rain_s_sunny.png";
                if (text.Contains("cloud")) return "rain_s_cloudy.png";
                return "rain.png";
            }

            // 4. Fog / Mist
            if (text.Contains("fog") || text.Contains("mist") || text.Contains("haze"))
                return "fog.png";

            // 5. Cloudy Logic
            if (text.Contains("cloud") || text.Contains("overcast"))
            {
                if (text.Contains("partly") || text.Contains("few") || text.Contains("broken"))
                    return "partly_cloudy.png";

                if (text.Contains("sun")) return "cloudy_s_sunny.png";
                return "cloudy.png";
            }

            // 6. Sunny / Clear
            if (text.Contains("clear") || text.Contains("sun"))
                return "sunny.png";

            // Fallback
            return "sunny.png";
        }
    }
}