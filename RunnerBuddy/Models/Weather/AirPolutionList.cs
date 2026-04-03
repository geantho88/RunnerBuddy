namespace RunnerBuddy.Models.Weather
{
    public class AirPollutionList
    {
        public AirQuality Main { get; set; } = new AirQuality();
        public AirComponents Components { get; set; } = new AirComponents();
    }
}
