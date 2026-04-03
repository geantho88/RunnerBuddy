using Microsoft.Extensions.Logging;
using RunnerBuddy.Models.AirPolution;
using RunnerBuddy.Models.Weather;
using RunnerBuddy.Services;
using System.Net.Http.Json;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger; // 1. Add Logger field

    private const string ApiKey = "b77bd67583c779bb7d3437c3afb24d46";
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

    // 2. Inject ILogger via constructor
    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}?lat={latitude}&lon={longitude}&appid={ApiKey}&units=metric";
            return await _httpClient.GetFromJsonAsync<WeatherData>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            return null;
        }
    }

    public async Task<AirPollutionData?> GetAirPollutionAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/air_pollution?lat={latitude}&lon={longitude}&appid={ApiKey}";
            return await _httpClient.GetFromJsonAsync<AirPollutionData>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching air pollution data for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            return null;
        }
    }
}