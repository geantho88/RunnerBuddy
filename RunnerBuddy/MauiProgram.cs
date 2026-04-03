using CommunityToolkit.Maui;
using Fonts;
using Microsoft.Extensions.Logging;
using RunnerBuddy.Pages;
using RunnerBuddy.Services;
using RunnerBuddy.ViewModels;
using Syncfusion.Maui.Toolkit.Hosting;

namespace RunnerBuddy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddHttpClient<IWeatherService, WeatherService>();
            builder.Services.AddSingleton<IWeatherService,WeatherService>();

            builder.Services.AddTransientWithShellRoute<MainPage, MainPageViewModel>("main");

            return builder.Build();
        }
    }
}
