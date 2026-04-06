using Azure;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RunnerBuddy.Models.RecommendationPlan;
using RunnerBuddy.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RunnerBuddy.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly ILogger<MainPageViewModel> _logger;
    private readonly IWeatherService _weatherService;
    private readonly IChatClient _chatClient;
    private Location _currentLocation;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string location = "Locating...";
    [ObservableProperty] private string temperature = "--";
    [ObservableProperty] private string humidity = "--";
    [ObservableProperty] private string weatherDescription = "--";
    [ObservableProperty] private string airQuality = "--";
    [ObservableProperty] private string airQualityAdditionalInfos = "--";
    [ObservableProperty] private RunningPlan recommendationPlan = new RunningPlan();

    public MainPageViewModel(ILogger<MainPageViewModel> logger, IWeatherService weatherService, IChatClient chatClient)
    {
        _logger = logger;
        _weatherService = weatherService;
        _chatClient = chatClient;
    }

    /// <summary>
    /// The primary entry point for the UI.
    /// Handles the entire sequence: Permissions -> Location -> Weather -> AI
    /// </summary>
    [RelayCommand]
    private async Task RefreshAllDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            if (await EnsureLocationPermissionsAsync())
            {
                await GetCurrentLocationAsync();

                await LoadWeatherAsync();
                await GetRecommendationsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full refresh cycle failed");
            await Shell.Current.DisplayAlertAsync("Error", "Buddy tripped! Check your connection and GPS.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> EnsureLocationPermissionsAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            Location = "Location access denied";
            return false;
        }
        return true;
    }

    private async Task GetCurrentLocationAsync()
    {
        _currentLocation = await Geolocation.Default.GetLastKnownLocationAsync()?? await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
    }

    private async Task LoadWeatherAsync()
    {
        if (_currentLocation == null) return;

        var weather = await _weatherService.GetWeatherAsync(_currentLocation.Latitude, _currentLocation.Longitude);
        if (weather != null)
        {
            Temperature = $"{weather.Main.Temp:F0}°C";
            Humidity = $"{weather.Main.Humidity}%";
            WeatherDescription = weather.Weather.FirstOrDefault()?.Description?.ToUpper() ?? "CLEAR";
            Location = weather.Name;
        }

        var air = await _weatherService.GetAirPollutionAsync(_currentLocation.Latitude, _currentLocation.Longitude);
        var airResult = air?.List?.FirstOrDefault();
        if (airResult != null)
        {
            AirQuality = GetAqiDescription(airResult.Main.Aqi);
            var comp = airResult.Components;
            AirQualityAdditionalInfos = $"CO: {comp.Co} | PM2.5: {comp.Pm2_5:F1} | PM10: {comp.Pm10:F1} | NO2: {comp.No2:F1}";
        }
    }

    private async Task GetRecommendationsAsync()
    {
        if (_currentLocation == null)
        {
            return;
        }

        // Structured Prompt for 2026 AI standards
        // Get the current date in a readable format
        var today = DateTime.Now.ToString("dddd, MMM dd, yyyy");

        var prompt = $@"
                    You are 'RunnerBuddy', an expert running coach. 
                    Today's Date: {today} 
                    Location Context: {Location} (Lat: {_currentLocation.Latitude}, Lon: {_currentLocation.Longitude})
                    Current Weather: {Temperature}, {WeatherDescription}, {Humidity} Humidity.
                    Air Quality Index: {AirQuality}.

                    TASK: Starting from TODAY ({today}), suggest optimal running windows for the next 7 days...
    
                    STRICT TIME CONSTRAINTS:
                    - Only suggest 'best_start_time' between 08:00 AM and 12:00 AM (Midnight).
                    - Do not suggest early morning runs before 08:00 AM.

                    STRICT REQUIREMENT: Return ONLY a raw JSON object. No markdown blocks.
                    SCHEMA:
                    {{
                        ""location"": ""{Location}"",
                        ""overall_score"": 0-100,
                        ""buddy_tip"": ""string"",
                        ""daily_suggestions"": [
                            {{ ""day of the week name and short date"": ""string"", ""best_start_time"": ""string"", ""Weather Description"":""string"", ""reason"": ""string"", ""aqi"": 0 }}
                        ]
                    }}";

        //var response = await _chatClient.GetResponseAsync<RunningPlan>(prompt);
        //RecommendationPlan = response.Result;

        var jsonResponse = "{\"location\":\"Kalamaria\",\"overallScore\":85,\"buddyTip\":\"Opt for running during times when the weather conditions and air quality are favorable.\",\"dailySuggestions\":[{\"day\":\"Δευτέρα, Απρ 06, 2026\",\"bestStartTime\":\"20:00\",\"reason\":\"Clear weather and moderate AQI during this time.\",\"weatherDescription\":\"Few clouds, 22°C, humidity 40%.\",\"aqi\":75},{\"day\":\"Τρίτη, Απρ 07, 2026\",\"bestStartTime\":\"19:30\",\"reason\":\"Comfortable evening conditions with fewer atmospheric pollutants.\",\"weatherDescription\":\"Partly cloudy, 20°C, humidity 43%.\",\"aqi\":70},{\"day\":\"Τετάρτη, Απρ 08, 2026\",\"bestStartTime\":\"18:45\",\"reason\":\"Optimum temperature and moderate air quality.\",\"weatherDescription\":\"Cloudy, 21°C, humidity 45%.\",\"aqi\":72},{\"day\":\"Πέμπτη, Απρ 09, 2026\",\"bestStartTime\":\"20:15\",\"reason\":\"Late evening provides balanced humidity and AQI levels.\",\"weatherDescription\":\"Clear sky, 22°C, humidity 38%.\",\"aqi\":68},{\"day\":\"Παρασκευή, Απρ 10, 2026\",\"bestStartTime\":\"19:15\",\"reason\":\"Weather stabilizes for evening runs.\",\"weatherDescription\":\"Few clouds, 20°C, humidity 40%.\",\"aqi\":73},{\"day\":\"Σάββατο, Απρ 11, 2026\",\"bestStartTime\":\"18:50\",\"reason\":\"Best conditions for prolonged runs.\",\"weatherDescription\":\"Clear sky, 22°C, humidity 39%.\",\"aqi\":71},{\"day\":\"Κυριακή, Απρ 12, 2026\",\"bestStartTime\":\"19:00\",\"reason\":\"Improved air conditions expected.\",\"weatherDescription\":\"Partly cloudy, 21°C, humidity 41%.\",\"aqi\":69}]}";
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        RecommendationPlan = JsonSerializer.Deserialize<RunningPlan>(jsonResponse, options);
    }

    private string GetAqiDescription(int aqi) => aqi switch
    {
        1 => "Good 🟢",
        2 => "Fair 🟡",
        3 => "Moderate 🟠",
        4 => "Poor 🔴",
        5 => "Very Poor 💀",
        _ => "Unknown"
    };
}