//using AndroidX.ConstraintLayout.Helper.Widget;
using AstroWeather.Helpers;
using Microsoft.Maui.Controls;
namespace AstroWeather
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            WeatherRouter getWeatherinfo = new();
            var Pogoda = getWeatherinfo.getWeatherinfo();
            if (Pogoda.Count != 0)
            {
                DateTime currentDateTime = DateTime.Now;
                var result2 = Pogoda.SelectMany(i => i).Distinct();
                var filteredWeather = result2.Skip(Convert.ToInt32(currentDateTime.Hour)).Take(12).ToList();
                BindingContext = new { pogoda = filteredWeather };
                ActualTemp.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].temp.ToString() + " °C";

                string weatherImage = WeatherRouter.GetWeatherImage();

                // Ustaw obraz w kontrolce Image
                WeatherImage.Source = weatherImage;
            }
           
        }
    }

}
