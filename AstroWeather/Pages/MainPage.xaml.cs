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
            DateTime currentDateTime = DateTime.Now;
            var result2 = Pogoda.SelectMany(i => i).Distinct();
            var filteredWeather = result2.Skip(Convert.ToInt32(currentDateTime.Hour)).Take(12).ToList();
            BindingContext = new { pogoda = filteredWeather };
            //ActualTemp.Text = lista[0][Convert.ToInt32(currentDateTime.Hour)].temp.ToString() + " °C";
            /*LocalisationListView.ItemsSource = lista[0].Select(kvp => new
            {
                Key = kvp.datetime,
                Value = kvp.cloudcover.ToString()
            }).ToList();*/
        }
    }

}
