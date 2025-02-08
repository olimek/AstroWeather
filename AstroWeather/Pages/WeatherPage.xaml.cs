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
        private static List<DayWithHours>? carouselDATA = new List<DayWithHours>();
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
            //LoadWeatherData();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefrescarouselIfNeeded();
        }

        private async Task RefrescarouselIfNeeded()
        {
            if ((DateTime.Now - LastcarouselUpdateTime).TotalHours >= 1)
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
            
            var carousel = await Helpers.WeatherRouter.GetCarouselViewAsync();
            carouselDATA = carousel.ToList();
            weatherCarousel.ItemsSource = carousel;

            int selectedIndex = 0;
            DateTime targetDate;
            if (!DateTime.TryParse(SelectedDay?.datetime, out targetDate))
            {
                // Jeśli parsowanie się nie uda, przyjmujemy aktualną datę.
                targetDate = DateTime.Now;
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = 0;
                TimeSpan minDelta = TimeSpan.MaxValue;

                // Zakładamy, że elementy carouselu to obiekty typu Day i mają właściwość "datetime" (string).
                for (int i = 0; i < carousel.Count; i++)
                {
                    DateTime itemDate;
                    if (DateTime.TryParseExact(carousel[i].Date, "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out itemDate))
                    {
                        // Obliczamy wartość bezwzględną różnicy między datami.
                        TimeSpan delta = (itemDate - targetDate).Duration();

                        if (delta < minDelta)
                        {
                            minDelta = delta;
                            selectedIndex = i;
                        }
                    }
                }
            }
            await Task.Delay(100);
            weatherCarousel.Loaded += (s, e) =>
            {

                var targetItem = carousel[selectedIndex];
                weatherCarousel.ScrollTo(targetItem, ScrollToPosition.Center, animate: true);
                Task.Delay(10);
                indicatorView.Position = selectedIndex;
            
            };
            Debug.WriteLine($"Wybrano indeks: {selectedIndex}, data: {carousel[selectedIndex].Date}");
        }

    }
}
