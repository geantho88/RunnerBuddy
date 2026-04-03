using RunnerBuddy.ViewModels;

namespace RunnerBuddy.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _model;

        public MainPage(MainPageViewModel model)
        {
            InitializeComponent();
            _model = model;
            BindingContext = _model;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_model != null)
            {
                await _model.LoadWeatherCommand.ExecuteAsync(null);
            }
        }
    }
}