using Azure.AI.OpenAI;
using CommunityToolkit.Maui;
using Fonts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RunnerBuddy.Pages;
using RunnerBuddy.Services;
using RunnerBuddy.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Reflection;

namespace RunnerBuddy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
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
#endif

            builder.Services.AddHttpClient<IWeatherService, WeatherService>();
            builder.Services.AddSingleton<IWeatherService,WeatherService>();

            builder.Services.AddTransientWithShellRoute<MainPage, MainPageViewModel>("main");

            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("RunnerBuddy.AppSettings.json");

                if (stream == null)
                {
                    throw new FileNotFoundException("Could not find AppSettings.json. Ensure 'Build Action' is 'Embedded Resource'.");
                }

                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                var endpoint = config["AzureOpenAI:Endpoint"];
                var apiKey = config["AzureOpenAI:ApiKey"];
                var model = config["AzureOpenAI:Model"] ?? "gpt-4o-mini";

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("AzureOpenAI settings are missing in AppSettings.json");
                }

                //This is model - level API, not agent-level API - Most apps are better with one smart agent
                var client = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(apiKey));
                return client.GetChatClient(model).AsIChatClient();
            });

            return builder.Build();
        }
    }
}
