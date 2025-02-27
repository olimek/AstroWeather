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
        private List<DayWithHours>? carouselDATA = new List<DayWithHours>();
        private static DateTime LastcarouselUpdateTime = DateTime.MinValue; // kiedy ostatnio pobrano dane
        private AstroWeather.Helpers.Day? _selectedDay;
        public AstroWeather.Helpers.Day SelectedDay
        {
            get => _selectedDay!;
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
            // Gdy wchodzimy na stronę, sprawdzamy, czy trzeba pobrać dane
            await RefrescarouselIfNeeded();
        }

        /// <summary>
        /// Sprawdza, czy minęło >= 1h od ostatniego pobrania.
        /// Jeśli tak – wyświetla popup i pobiera dane od nowa.
        /// W przeciwnym razie używa już pobranych danych (carouselDATA).
        /// </summary>
        private async Task RefrescarouselIfNeeded()
        {
            // Czy minęła >= 1h od ostatniego pobrania?
            if ((DateTime.UtcNow - LastcarouselUpdateTime).TotalHours >= 1)
            {
                // Wyświetlamy popup (tylko przy nowym pobraniu)
                await Navigation.PushModalAsync(new AstroWeather.Pages.PopUp("Pobieranie pogody"));
                await LoadWeatherData();  // faktyczne pobranie

              
            }
            else
            {
                // Używamy już pobranych danych
                BindingContext = new { weather = carouselDATA };
            }
        }

        /// <summary>
        /// Pobiera dane z API i ustawia je w carouselDATA.
        /// </summary>
        private async Task LoadWeatherData()
        {
            if (!WeatherRouter.IsApiVariables())
            {
                // Brak konfiguracji API
                await Dispatcher.DispatchAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//SettingsPage");
                });
            }
            else
            {
                var weatherRouter = new WeatherRouter();

                var carousel = await weatherRouter.GetCarouselViewAsync();
                carouselDATA = carousel?.ToList() ?? new List<DayWithHours>();
                weatherCarousel.ItemsSource = carouselDATA;

                // Znajdujemy indeks najbliższy dacie selectedDay
                int selectedIndex = 0;
                if (!DateTime.TryParse(SelectedDay?.datetime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime targetDate))
                {
                    // jeśli parse się nie udał, używamy DateTime.UtcNow
                    targetDate = DateTime.UtcNow;
                }

                TimeSpan minDelta = TimeSpan.MaxValue;
                for (int i = 0; i < carouselDATA.Count; i++)
                {
                    if (DateTime.TryParseExact(carouselDATA[i].Date, "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime itemDate))
                    {
                        TimeSpan delta = (itemDate - targetDate).Duration();
                        if (delta < minDelta)
                        {
                            minDelta = delta;
                            selectedIndex = i;
                        }
                    }
                }

                // Po załadowaniu listy przesuwamy do wybranego indeksu
                weatherCarousel.Loaded += (s, e) =>
                {
                    var targetItem = carouselDATA[selectedIndex];
                    weatherCarousel.ScrollTo(targetItem, ScrollToPosition.Center, animate: false);
                    indicatorView.Position = selectedIndex;
                };

                // Ustawiamy czas ostatniego pobrania na teraz
                LastcarouselUpdateTime = DateTime.UtcNow;

                // Jeśli chcesz odświeżyć BindingContext:
                BindingContext = new { weather = carouselDATA };
            }
        }
    }
}
