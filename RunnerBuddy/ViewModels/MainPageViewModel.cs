using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RunnerBuddy.Services;

namespace RunnerBuddy.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IWeatherService _weatherService;
        public IAsyncRelayCommand LoadWeatherCommand { get; }

        [ObservableProperty] private string location = "Unknown";
        [ObservableProperty] private string temperature = "--";
        [ObservableProperty] private string humidity = "--";
        [ObservableProperty] private string weatherDescription = "--";
        [ObservableProperty] private string airQuality = "--";
        [ObservableProperty] private string airQualityAdditionalInfos = "--";

        public MainPageViewModel(IWeatherService weatherService)
        {
            _weatherService = weatherService;
            LoadWeatherCommand = new AsyncRelayCommand(LoadWeatherAsync);
        }

        private async Task LoadWeatherAsync()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    Location = "Location denied";
                    return;
                }

                var loc = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

                if (loc != null)
                {
                    // Store coordinates
                    Preferences.Set("latitude", loc.Latitude);
                    Preferences.Set("longitude", loc.Longitude);

                    Location = $"{loc.Latitude:F4}, {loc.Longitude:F4}";

                    // Get weather
                    var weather = await _weatherService.GetWeatherAsync(loc.Latitude, loc.Longitude);
                    if (weather != null)
                    {
                        Temperature = $"{weather.Main.Temp:F1}°C";
                        Humidity = $"{weather.Main.Humidity}%";
                        WeatherDescription = weather.Weather.FirstOrDefault()?.Main ?? "--";
                        Location = weather.Name; // override with city name if available
                    }

                    // Get air pollution
                    var airPollution = await _weatherService.GetAirPollutionAsync(loc.Latitude, loc.Longitude);
                    if (airPollution != null)
                    {
                        AirQuality = airPollution.List.SingleOrDefault()?.Main.Aqi switch
                        {
                            1 => "Good",
                            2 => "Fair",
                            3 => "Moderate",
                            4 => "Poor",
                            5 => "Very Poor",
                            _ => "--"
                        };

                        var airPollutionInfo = airPollution.List.SingleOrDefault()?.Components;
                        AirQualityAdditionalInfos = $"CO: {airPollutionInfo?.Co}, NO: {airPollutionInfo?.No}, NO2: {airPollutionInfo?.No2}, SO2: {airPollutionInfo?.So2}";
                    }
                }
            }
            catch
            {
                Location = "Error fetching data";
                Temperature = "--";
                Humidity = "--";
                WeatherDescription = "--";
                AirQuality = "--";
            }
        }
    }
}