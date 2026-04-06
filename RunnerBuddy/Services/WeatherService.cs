using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RunnerBuddy.Models.AirPolution;
using RunnerBuddy.Models.Weather;
using RunnerBuddy.Services;
using System.Net.Http.Json;
using System.Reflection;


public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger; // 1. Add Logger field

    private readonly string _apiKey;
    private readonly string _baseUrl;

    // 2. Inject ILogger via constructor
    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RunnerBuddy.AppSettings.json");
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        _apiKey = config["OpenWeather:ApiKey"];
        _baseUrl = config["OpenWeather:BaseUrl"];
    }

    public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{_baseUrl}?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
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
            var url = $"https://api.openweathermap.org/data/2.5/air_pollution?lat={latitude}&lon={longitude}&appid={_apiKey}";
            return await _httpClient.GetFromJsonAsync<AirPollutionData>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching air pollution data for Lat: {Lat}, Lon: {Lon}", latitude, longitude);
            return null;
        }
    }
}