using AstroWeather.Helpers;
using CosineKitty;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace AstroWeather.Pages
{
    // Dzięki [QueryProperty] parametr "SelectedWeatherItem" zostanie przekazany do tej strony.
    [QueryProperty(nameof(SelectedDay), "selectedDay")]
    public partial class WeatherPage : ContentPage
    {
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
            await LoadWeatherData();
        }
        private async Task LoadWeatherData()
        {
            // Pobierz dane do CarouselView (przyjmujemy, że zwraca listę obiektów typu Day)
            var carousel = await Helpers.WeatherRouter.GetCarouselViewAsync();
            weatherCarousel.ItemsSource = carousel;

            // Parsujemy datę przekazaną w SelectedWeatherItem.
            // Zakładamy, że SelectedWeatherItem.datetime zawiera datę w formacie np. "2025-02-02"
            DateTime targetDate;
           if (!DateTime.TryParse(SelectedDay?.datetime, out targetDate))
            {
                // Jeśli parsowanie się nie uda, przyjmujemy aktualną datę.
                targetDate = DateTime.Now;
            }

            // Znajdź element w carouselu, którego data (parsowana z właściwości datetime) jest najbliższa targetDate.
            int selectedIndex = 0;
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
            
            weatherCarousel.Loaded += (s, e) =>
            {
                weatherCarousel.ScrollTo(selectedIndex, animate: true);
            };
            
        }

    }
}
