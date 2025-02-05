using AstroWeather.Helpers;
using Microsoft.Maui.Controls;
using CosineKitty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AstroWeather
{
    public partial class MainPage : ContentPage
    {
        public static List<AstroWeather.Helpers.Day> GlobalWeatherList = new List<AstroWeather.Helpers.Day>();
        private static DateTime LastWeatherUpdateTime = DateTime.MinValue;
        private readonly LogFileGetSet _logFileGetSet = new LogFileGetSet();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshWeatherIfNeeded();
        }

        private async Task RefreshWeatherIfNeeded()
        {
            if ((DateTime.Now - LastWeatherUpdateTime).TotalHours >= 1)
            {
                await MainInit();
            }
            else
            {
                BindingContext = new { weather = GlobalWeatherList };
            }
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
                GlobalWeatherList = pogodaDzienna.ToList();

                LastWeatherUpdateTime = DateTime.Now;
                BindingContext = new { weather = GlobalWeatherList };

                double phase = Astronomy.MoonPhase(time);

                MoonImage.Source = WeatherRouter.GetMoonImage(Math.Round(100.0 * illum.phase_fraction), phase);
                var defaultLocName = await _logFileGetSet.LoadDefaultLocNameAsync();
                SecondLabel.Text = defaultLocName ?? "Default location name not found";

                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private async void WeatherListView_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is AstroWeather.Helpers.Day selectedItem)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "selectedDay", selectedItem }
                };

                await Shell.Current.GoToAsync("//WeatherPage", parameters);
            }

            ((CollectionView)sender).SelectedItem = null;
        }
    }
}