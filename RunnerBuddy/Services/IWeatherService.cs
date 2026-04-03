using RunnerBuddy.Models.AirPolution;
using RunnerBuddy.Models.Weather;

namespace RunnerBuddy.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetWeatherAsync(double latitude, double longitude);
        Task<AirPollutionData?> GetAirPollutionAsync(double latitude, double longitude);
    }
}
