
using System.Text.Json;
using AstroWeather.Helpers;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
using Innovative.SolarCalculator;
namespace AstroWeather
{

    public partial class MainPage : ContentPage
    {
                public MainPage()
        {
            InitializeComponent();
            WeatherRouter getWeatherinfo = new();
            var lista = getWeatherinfo.getWeatherinfo();
            DateTime currentDateTime = DateTime.Now;
            ;

            ActualTemp.Text = lista[0][Convert.ToInt32(currentDateTime.Hour)].temp.ToString() + " °C";
        }
        

        

        
    }
}
