using AstroWeather.Helpers;
using Microsoft.Maui.Controls;
using CosineKitty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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
            await drawMoonGraph(0);
        }

        private async Task RefreshWeatherIfNeeded()
        {
            if ((DateTime.Now - LastWeatherUpdateTime).TotalHours >= 1)
            {
                await MainInit();
            }
            else
            {   if (GlobalWeatherList.Count <= 3) { await MainInit(); }
                else
                {
                    BindingContext = new { weather = GlobalWeatherList };
                }   
                
            }
        }
        private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            int firstVisibleIndex = e.FirstVisibleItemIndex;
            drawMoonGraph(firstVisibleIndex);

        }
        private async Task drawMoonGraph(int firstVisibleIndex)
        {
            DateTime parsedDate;
            if (GlobalWeatherList.Count <= 3) {
               
                await Shell.Current.GoToAsync("//SettingsPage");
               
            }
            else
            {
                DateTime.TryParseExact(GlobalWeatherList[firstVisibleIndex].datetime, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate);

                var time = new AstroTime(parsedDate);
                double phase = Astronomy.MoonPhase(time);

                IllumInfo illum = Astronomy.Illumination(Body.Moon, time);
                MoonImage.Source = WeatherRouter.GetMoonImage(Math.Round(100.0 * illum.phase_fraction), phase);
                var moontimes = WeatherRouter.GetAstroTimes(parsedDate, true);
                var moonSet = moontimes[4];
                var moonrise = moontimes[5];
                TimeSpan nightDuration = moontimes[3] - moontimes[2];
                ActualTemp.Text = $"Data: {parsedDate.ToString("dd-MM")}, Długość nocy: {Math.Round(nightDuration.TotalHours, 1)}";
                ActualHum.Text = $"Moonrise: {moonrise.ToString("HH:mm dd-MM")}, Moonset: {moonSet.ToString("HH:mm dd-MM")}";
                Actualpress.Text = $"Moon illumination: {Math.Round(100.0 * illum.phase_fraction, 1)} %";
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


                var pogodaDzienna = await WeatherRouter.SetWeatherBindingContextAsync();
                GlobalWeatherList = pogodaDzienna.ToList();

                LastWeatherUpdateTime = DateTime.Now;
                BindingContext = new { weather = GlobalWeatherList };


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