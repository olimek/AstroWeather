using CosineKitty;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace AstroWeather.Pages
{
    public partial class WeatherPage : ContentPage
    {
        public WeatherPage()
        {
            InitializeComponent();
            LoadWeatherData();
        }

        private async void LoadWeatherData()
        {
            var carousel = await Helpers.WeatherRouter.GetCarouselViewAsync();
            weatherCarousel.ItemsSource = carousel;
            weatherCarousel.ScrollTo(0);
        }
    }
}