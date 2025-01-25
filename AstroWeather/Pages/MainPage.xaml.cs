//using AndroidX.ConstraintLayout.Helper.Widget;
using AstroWeather.Helpers;
using Microsoft.Maui.Controls;
using AstroWeather.Pages;
using CosineKitty;
namespace AstroWeather
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            MainInit();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            MainInit();
        }
        async void MainInit()
        {
            var IsAPI = LogFileGetSet.GetAPIKey("weather");
            var IsDefLoc = LogFileGetSet.LoadDefaultLoc();
            if (IsAPI == null || IsDefLoc == null)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("//SettingsPage");
                });
            }
            else
            {

                WeatherRouter getWeatherinfo = new();
                List<List<AstroWeather.Helpers.Hour>> Pogoda = getWeatherinfo.getWeatherinfo();
                if (Pogoda.Count != 0)
                {
                    DateTime currentDateTime = DateTime.Now;
                    var result2 = Pogoda.SelectMany(i => i).Distinct();
                    var filteredWeather = result2.Skip(Convert.ToInt32(currentDateTime.Hour)).Take(12).ToList();


                    BindingContext = new { pogoda = filteredWeather };
                    ActualTemp.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].temp.ToString() + " °C";


                    var time = new AstroTime(DateTime.UtcNow);
                    IllumInfo illum = Astronomy.Illumination(Body.Moon, time);
                    Console.WriteLine("{0} : Moon's illuminated fraction = {1:F2}%.", time, Math.Round(100.0 * illum.phase_fraction), 1);



                    var ss = WeatherRouter.SetWeatherdata(result2.ToList());
                    var Warunkihodzinowe = WeatherRouter.CalculateWeatherdata(Pogoda);
                    var Warunkidzienne = WeatherRouter.CalculateAstroNight(Pogoda);
                    var dzienne = WeatherRouter.getCalculatedDaily(Pogoda);
                    string weatherImage = WeatherRouter.GetWeatherImage();
                    BindingContext = new { weather = dzienne };

                    double phase = Astronomy.MoonPhase(time);
                    
                    MoonImage.Source = WeatherRouter.GetMoonImage(Math.Round(100.0 * illum.phase_fraction),phase);
                    SecondLabel.Text = LogFileGetSet.LoadDefaultLocName();

                    Actualpress.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].pressure.ToString() + " hPa";
                    ActualHum.Text = Pogoda[0][Convert.ToInt32(currentDateTime.Hour)].humidity.ToString() + " %";
                    await Shell.Current.GoToAsync("//MainPage");
                }
            }
        }
    }

}
