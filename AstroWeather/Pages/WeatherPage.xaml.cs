using System.Globalization;
using AstroWeather.Helpers;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace AstroWeather.Pages
{
    [QueryProperty(nameof(SelectedDay), "selectedDay")]
    public partial class WeatherPage : ContentPage
    {
        private List<DayWithHours>? carouselDATA = new List<DayWithHours>();
        private static DateTime LastcarouselUpdateTime = DateTime.MinValue;
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
            await RefrescarouselIfNeeded();
        }
        private void OnCanvasPaintSurface(object s, SKPaintSurfaceEventArgs e)
        {
            var day = (DayWithHours)((SKCanvasView)s).BindingContext;
            DateTime date = DateTime.ParseExact(day.Date, "dd.MM", CultureInfo.InvariantCulture);
            WeatherRouter.DrawNightTimeline(e.Surface.Canvas, e.Info, date, true, int.Parse(day.moonilum));
        }


        private async Task RefrescarouselIfNeeded()
        {
            if ((DateTime.UtcNow - LastcarouselUpdateTime).TotalHours >= 1)
            {
                //await Navigation.PushModalAsync(new AstroWeather.Pages.PopUp("Pobieranie pogody"));
                await LoadWeatherData();
            }
            else
            {
                BindingContext = new { weather = carouselDATA };
                // Dodajemy wywołanie przewinięcia, gdy dane są już załadowane
                await ScrollToSelectedDay();
            }
        }

        private async Task LoadWeatherData()
        {
            if (!WeatherRouter.IsApiVariables())
            {
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

                // Przewijamy carousel do wybranego dnia po załadowaniu danych
                await ScrollToSelectedDay();

                LastcarouselUpdateTime = DateTime.UtcNow;
                BindingContext = new { weather = carouselDATA };
            }
        }

        private async Task ScrollToSelectedDay()
        {
            int selectedIndex = 0;
            if (!DateTime.TryParse(SelectedDay?.datetime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime targetDate))
            {
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
            if (carouselDATA.Count > selectedIndex)
            {
                weatherCarousel.ScrollTo(carouselDATA[selectedIndex], ScrollToPosition.Center, animate: false);
                indicatorView.Position = selectedIndex;
            }
        }
    }
}