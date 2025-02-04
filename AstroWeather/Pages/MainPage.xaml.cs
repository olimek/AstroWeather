using AstroWeather.Helpers;
using Microsoft.Maui.Controls;
using CosineKitty;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AstroWeather
{
    public partial class MainPage : ContentPage
    {
        private readonly LogFileGetSet _logFileGetSet = new LogFileGetSet();
        public MainPage()
        {
            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await MainInit();
        }

        private async Task MainInit()
        {
            var isAPI = await _logFileGetSet.GetAPIKeyAsync("weather");
            var isDefLoc = await _logFileGetSet.LoadDefaultLocAsync();

            if (isAPI == null || isDefLoc == null)
            {
                await Dispatcher.DispatchAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//SettingsPage");
                });
            }
            else
            {

                DateTime currentDateTime = DateTime.Now;


                var time = new AstroTime(DateTime.UtcNow);
                IllumInfo illum = Astronomy.Illumination(Body.Moon, time);

                string weatherImage = WeatherRouter.GetWeatherImage(illum.phase_fraction);
                var pogodaDzienna = await WeatherRouter.SetWeatherBindingContextAsync();
                BindingContext = new { weather = pogodaDzienna };

                double phase = Astronomy.MoonPhase(time);

                MoonImage.Source = WeatherRouter.GetMoonImage(Math.Round(100.0 * illum.phase_fraction), phase);
                var defaultLocName = await _logFileGetSet.LoadDefaultLocNameAsync();
                SecondLabel.Text = defaultLocName ?? "Default location name not found";


                await Shell.Current.GoToAsync("//MainPage");

            }
        }
        private async void WeatherListView_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            //await DisplayAlert("Info", "Kliknięto element", "OK");
            if (e.CurrentSelection?.FirstOrDefault() is AstroWeather.Helpers.Day selectedItem)
            {
                var parameters = new Dictionary<string, object>
    {
        { "selectedDay", selectedItem } // Klucz musi pasować do [QueryProperty]
    };

                await Shell.Current.GoToAsync("//WeatherPage", parameters);
            }

    ((CollectionView)sender).SelectedItem = null;
        }
    }
}