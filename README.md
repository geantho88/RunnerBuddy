**🏃‍♂️ RunnerBuddy**
AI-Powered Fitness Companion for .NET MAUI & Azure AI Foundry

Live from Thessaloniki! 🇬🇷 This repo contains the source code for my session at Global Azure 2026. We are moving past basic "Hello World" apps and building a mobile experience that thinks, reacts, and motivates.

**⚡ The Vision**
Most fitness apps just track data. RunnerBuddy interprets it. By combining cross-platform UI with Azure's LLM orchestration, this app tells you why you should run today, not just how far you went.

**Core Pillars**
Intelligent Context: Uses real-time weather, AQI (Air Quality), and location data.

Azure AI Foundry: Leverages GPT-4o models to generate personalized "City Vibes" and coaching advice.

High Performance UI: Built with the CommunityToolkit.Mvvm and Syncfusion Toolkit for a snappy, native feel.

**The appsettings.json file is excluded from this repository because it contains sensitive API secrets. To run this application locally, create an appsettings.json file in the root directory and populate it with your credentials:**

JSON
{
  "OpenWeather": {
    "ApiKey": "YOUR_OPENWEATHER_API_KEY",
    "BaseUrl": "https://api.openweathermap.org/data/2.5/weather"
  },
  "AzureOpenAI": {
    "Endpoint": "YOUR_AZURE_ENDPOINT",
    "ApiKey": "YOUR_AZURE_API_KEY",
    "Model": "gpt-4o"
  }
}
