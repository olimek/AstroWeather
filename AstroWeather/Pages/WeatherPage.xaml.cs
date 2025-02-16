using AstroWeather.Helpers;
using CosineKitty;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace AstroWeather.Pages
{

    [QueryProperty(nameof(SelectedDay), "selectedDay")]
    public partial class WeatherPage : ContentPage
    {
        private static List<DayWithHours>? carouselDATA = new();
        private static DateTime LastcarouselUpdateTime = DateTime.MinValue;
        private AstroWeather.Helpers.Day _selectedDay;
        public AstroWeather.Helpers.Day SelectedDay
        {
            get => _selectedDay;
            set
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
            }
        }

        public WeatherPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefrescarouselIfNeeded();
        }

        private async Task RefrescarouselIfNeeded()
        {
            if ((DateTime.UtcNow - LastcarouselUpdateTime).TotalHours >= 1)
            {
                await LoadWeatherData();
            }
            else
            {
                BindingContext = new { weather = carouselDATA };
            }
        }

        private async Task LoadWeatherData()
        {
            var weatherRouter = new WeatherRouter();
            var carousel = await weatherRouter.GetCarouselViewAsync();

            carouselDATA = carousel?.ToList() ?? new List<DayWithHours>();

            weatherCarousel.ItemsSource = carouselDATA;

            int selectedIndex;
            DateTime targetDate;
            if (!DateTime.TryParse(SelectedDay?.datetime, out targetDate))
            {
                targetDate = DateTime.UtcNow;
                selectedIndex = 0;
            }

            selectedIndex = 0;
            TimeSpan minDelta = TimeSpan.MaxValue;

            for (int i = 0; i < carouselDATA.Count; i++)
            {
                DateTime itemDate;
                if (DateTime.TryParseExact(carouselDATA[i].Date, "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out itemDate))
                {
                    TimeSpan delta = (itemDate - targetDate).Duration();

                    if (delta < minDelta)
                    {
                        minDelta = delta;
                        selectedIndex = i;
                    }
                }
            }
            await Task.Delay(100);
            weatherCarousel.Loaded += (s, e) =>
            {
                var targetItem = carouselDATA[selectedIndex];
                weatherCarousel.ScrollTo(targetItem, ScrollToPosition.Center, animate: false);
                Task.Delay(10);
                indicatorView.Position = selectedIndex;
            };
        }
    }
}
