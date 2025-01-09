using AstroWeather.Helpers;
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
            var result2 = lista.SelectMany(i => i).Distinct();

            ActualTemp.Text = lista[0][Convert.ToInt32(currentDateTime.Hour)].temp.ToString() + " °C";
            LocalisationListView.ItemsSource = lista[0].Select(kvp => new
            {
                Key = kvp.datetime,
                Value = kvp.cloudcover.ToString()
            }).ToList();
        }
    }

}
