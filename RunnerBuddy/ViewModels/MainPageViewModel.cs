using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RunnerBuddy.Models.RecommendationPlan;
using RunnerBuddy.Services;
using System.Text;
using System.Text.Json;

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
    [ObservableProperty] private string userInstructions = string.Empty;
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
        if (IsBusy)
        {
            return;
        }

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
            await Shell.Current.DisplayAlertAsync("Error", $"Buddy failed! {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshRunningScheduleAsync()
    {
        try
        {
            IsBusy = true;
            await GetRecommendationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh running schedule failed");
            await Shell.Current.DisplayAlertAsync("Error", $"Buddy failed! {ex.Message}", "OK");
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
        _currentLocation = await Geolocation.Default.GetLastKnownLocationAsync() ?? await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
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

        var today = DateTime.Now.ToString("dddd, MMM dd, yyyy");

        var prompt = $@"
                    You are 'RunnerBuddy', an expert running coach. 
                    Today's Date: {today} 
                    Location Context: {Location} (Lat: {_currentLocation.Latitude}, Lon: {_currentLocation.Longitude})
                    Current Weather: {Temperature}, {WeatherDescription}, {Humidity} Humidity.
                    Air Quality Index: {AirQuality}.

                    TASK: Starting from TODAY ({today}), suggest optimal running windows for the next 7 days.

                    STRICT TIME CONSTRAINTS:
                    - Only suggest 'best_start_time' between 08:00 AM and 12:00 AM (Midnight).
                    - Do not suggest early morning runs before 08:00 AM.";

        // Inject User Overrides here
        if (!string.IsNullOrEmpty(UserInstructions))
        {
            prompt += $@"
                                USER SPECIFIC OVERRIDES:
                                - CONSIDER: The user has provided these specific constraints: '{UserInstructions}'. 
                                - You must prioritize these instructions over your standard coaching logic (e.g., if they say 'skip Wednesday', do not provide a suggestion for that day).";
        }

        prompt += @"
                            STRICT REQUIREMENT: Return ONLY a raw JSON object. No markdown blocks.
                            SCHEMA:
                            {
                                ""location"": ""{Location}"",
                                ""overall_score"": 0-100,
                                ""buddy_tip"": ""string"",
                                ""daily_suggestions"": [
                                    { ""day of the week name and short date"": ""string"", ""best_start_time"": ""string"", ""Weather Description"":""string"", ""reason"": ""string"", ""aqi"": 0 }
                                ],
                                ""location_activities"" : [{""alerts"":""string""}]
                            }";

        var response = await _chatClient.GetResponseAsync<RunningPlan>(prompt);
        RecommendationPlan = response.Result;

        //var jsonResponse = "{\"location\":\"Kalamaria\",\"overall_score\":75,\"buddy_tip\":\"Ensure to stay hydrated during your runs due to moderate air quality.\",\"daily_suggestions\":[{\"day\":\"Τετάρτη, Απρ 15, 2026\",\"best_start_time\":\"09:00 AM\",\"weather_description\":\"18°C, Overcast Clouds\",\"reason\":\"Comfortable temperature and permissible AQI (Moderate).\",\"aqi\":75},{\"day\":\"Πέμπτη, Απρ 16, 2026\",\"best_start_time\":\"07:30 PM\",\"weather_description\":\"19°C, Partly Cloudy\",\"reason\":\"Optimal evening conditions with cooler temperatures and permissible air quality.\",\"aqi\":70},{\"day\":\"Παρασκευή, Απρ 17, 2026\",\"best_start_time\":\"08:30 PM\",\"weather_description\":\"20°C, Clear Skies\",\"reason\":\"Pleasant clear skies for an evening jog.\",\"aqi\":65},{\"day\":\"Σάββατο, Απρ 18, 2026\",\"best_start_time\":\"10:00 AM\",\"weather_description\":\"22°C, Sunny\",\"reason\":\"Bright and sunny morning for a vibrant start.\",\"aqi\":60},{\"day\":\"Κυριακή, Απρ 19, 2026\",\"best_start_time\":\"06:00 PM\",\"weather_description\":\"23°C, Light Breeze\",\"reason\":\"Warm evening with pleasant breezes.\",\"aqi\":55},{\"day\":\"Δευτέρα, Απρ 20, 2026\",\"best_start_time\":\"09:00 PM\",\"weather_description\":\"18°C, Partly Cloudy\",\"reason\":\"Cool night perfect for a jog.\",\"aqi\":62},{\"day\":\"Τρίτη, Απρ 21, 2026\",\"best_start_time\":\"11:00 PM\",\"weather_description\":\"17°C, Clear Skies\",\"reason\":\"Late-night run with clear skies is refreshing.\",\"aqi\":60}],\"location_activities\":[\"Participate in the annual Kalamaria Urban Marathon this Sunday.\"]}";
        //var options = new JsonSerializerOptions
        //{
        //    PropertyNameCaseInsensitive = true
        //};
        //RecommendationPlan = JsonSerializer.Deserialize<RunningPlan>(jsonResponse, options);
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

    [RelayCommand]
    public async Task AddToCalendar(DaySuggestion suggestion)
    {
        if (suggestion == null)
        {
            return;
        }

        try
        {
            DateTime datePart = DateTime.Parse(suggestion.Day);
            DateTime timePart = DateTime.Parse(suggestion.BestStartTime);
            DateTime start = new DateTime(
                datePart.Year,
                datePart.Month,
                datePart.Day,
                timePart.Hour,
                timePart.Minute,
                0);

            DateTime end = start.AddHours(1);

            string startStr = start.ToString("yyyyMMddTHHmmss");
            string endStr = end.ToString("yyyyMMddTHHmmss");

            var icsBuilder = new StringBuilder();
            icsBuilder.AppendLine("BEGIN:VCALENDAR");
            icsBuilder.AppendLine("VERSION:2.0");
            icsBuilder.AppendLine("PRODID:-//RunnerBuddy//NONSGML v1.0//EN");
            icsBuilder.AppendLine("BEGIN:VEVENT");
            icsBuilder.AppendLine($"DTSTART:{startStr}");
            icsBuilder.AppendLine($"DTEND:{endStr}");
            icsBuilder.AppendLine($"SUMMARY:🏃 RunnerBuddy: {suggestion.WeatherDescription}");
            icsBuilder.AppendLine($"DESCRIPTION:{suggestion.Reason}");
            icsBuilder.AppendLine("LOCATION:Outdoors");
            icsBuilder.AppendLine("END:VEVENT");
            icsBuilder.AppendLine("END:VCALENDAR");

            string filePath = Path.Combine(FileSystem.CacheDirectory, "RunSchedule.ics");
            await File.WriteAllTextAsync(filePath, icsBuilder.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Add Run to Calendar",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full refresh cycle failed");
            await Shell.Current.DisplayAlertAsync("Error", $"Calendar Share Error: {ex.Message}", "OK");
        }
    }
}