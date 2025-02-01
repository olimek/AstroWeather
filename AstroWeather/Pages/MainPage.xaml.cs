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
            //MainInit();
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
                WeatherRouter getWeatherinfo = new();
                List<List<AstroWeather.Helpers.Hour>> Pogoda = await getWeatherinfo.GetWeatherInfoAsync();

                if (Pogoda.Count != 0)
                {
                    DateTime currentDateTime = DateTime.Now;
                    var result2 = Pogoda.SelectMany(i => i).Distinct();
                    var filteredWeather = result2.Skip(Convert.ToInt32(currentDateTime.Hour)).Take(12).ToList();

                    
                    ActualTemp.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].temp.ToString() + " °C";

                    var time = new AstroTime(DateTime.UtcNow);
                    IllumInfo illum = Astronomy.Illumination(Body.Moon, time);

                    var ss = WeatherRouter.SetWeatherData(result2.ToList());
                    var Warunkihodzinowe = WeatherRouter.CalculateWeatherData(Pogoda);
                    var Warunkidzienne = WeatherRouter.CalculateAstroNight(Pogoda);
                    var dzienne = await getWeatherinfo.GetCalculatedDailyAsync(Pogoda);

                    string weatherImage = WeatherRouter.GetWeatherImage(illum.phase_fraction);
                    BindingContext = new { weather = dzienne };

                    double phase = Astronomy.MoonPhase(time);

                    MoonImage.Source = WeatherRouter.GetMoonImage(Math.Round(100.0 * illum.phase_fraction), phase);
                    var defaultLocName = await _logFileGetSet.LoadDefaultLocNameAsync();
                    SecondLabel.Text = defaultLocName ?? "Default location name not found";

                    Actualpress.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].pressure.ToString() + " hPa";
                    ActualHum.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].humidity.ToString() + " %";

                    await Shell.Current.GoToAsync("//MainPage");
                }
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