namespace RunnerBuddy.Models.Weather
{
    public class WeatherData
    {
        public MainInfo Main { get; set; } = new MainInfo();
        public WeatherDescription[] Weather { get; set; } = Array.Empty<WeatherDescription>();
        public string Name { get; set; } = string.Empty;
    }
}
