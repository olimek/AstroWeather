using CosineKitty;
using Microsoft.Maui.Controls;

namespace AstroWeather.Pages;

public partial class WeatherPage : ContentPage
{
    public WeatherPage()
    {
        InitializeComponent();
        LoadWeatherData();
    }

    private void LoadWeatherData()
    {
            var carousel = Helpers.WeatherRouter.GetCarouselView();
            weatherCarousel.ItemsSource = carousel;
            
        
    }
}