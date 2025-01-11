using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AstroWeather.Helpers;
using System.Text.Json;
using Innovative.SolarCalculator;
using System.Globalization;

namespace AstroWeather.Helpers
{
    public class WeatherRouter

    {
        private WeatherAPI GetWeatherData()
        {

            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();
            if (DefaultLatLon != null) { 
            string APIkey = LogFileGetSet.GetAPIKey("weather");
            string LAT = DefaultLatLon[0].Replace(",", ".");
            string LON = DefaultLatLon[1].Replace(",", ".");
            string jsonresponse = ReadResponseFromUrl($"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{LAT}%2C%20{LON}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Ctemp%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cwindspeed%2Cpressure%2Ccloudcover%2Cvisibility&include=days%2Chours%2Cfcst%2Cobs&key={APIkey}&contentType=json");

            jsonresponse = jsonresponse.Replace("null", "0");

            WeatherAPI? weather = JsonSerializer.Deserialize<WeatherAPI>(jsonresponse);

            return weather; }
        else{return null;}
        }

        private Astro GetAstroData(string DATE)
        {
            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();

            // Sprawdzenie poprawności danych lokalizacji
            if (DefaultLatLon == null || DefaultLatLon.Count < 2)
            {
                throw new Exception("DefaultLatLon is null or incomplete.");
            }
            string APIkey = LogFileGetSet.GetAPIKey("astro");
            string LAT = DefaultLatLon[0].Replace(",", ".");
            string LON = DefaultLatLon[1].Replace(",", ".");
            string jsonresponse = ReadResponseFromUrl($"https://api.ipgeolocation.io/astronomy?apiKey={APIkey}&lat={LAT}&long={LON}&date={DATE}");
            Astro? astro = JsonSerializer.Deserialize<Astro>(jsonresponse);
            return astro;
        }

        /*
        TimeZoneInfo polandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        SolarTimes solarTimes = new SolarTimes(DateTime.Parse("2025-06-15"), 51.1080, 17.0385);
        DateTime sunrise = TimeZoneInfo.ConvertTimeFromUtc(solarTimes.DuskNautical.ToUniversalTime(), polandTimeZone);
        SolarTimes solarTimes1 = new SolarTimes(DateTime.Parse("2025-06-16"), 51.1080, 17.0385);
        DateTime sunset = TimeZoneInfo.ConvertTimeFromUtc(solarTimes1.DawnNautical.ToUniversalTime(), polandTimeZone);
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        string formattedDate = today.ToString("yyyy-MM-dd");\
        string formattedDate = "2025-06-15";
        var test = GetAstroData(formattedDate);
        moonrisel.Text = sunrise.ToString();
            moonsetl.Text = sunset.ToString();
            WeatherRouter weather = new WeatherRouter();
            var test2 =  weather.WeatherInit();

            var todayWeather = test2.days.FirstOrDefault(x => x.datetime == formattedDate);

            if (todayWeather != null)
            {
                // Pobranie właściwości z pominięciem "hours"
                var properties = todayWeather.GetType().GetProperties()
                    .Where(p => p.Name != "hours" && p.Name != "datetimeEpoch")
                    .ToDictionary(p => p.Name, p => p.GetValue(todayWeather)?.ToString());

                // Ustawienie danych jako źródło dla ListView
                weatherListView.ItemsSource = properties.Select(kvp => new
                {
                    Key = kvp.Key,
                    Value = kvp.Value
                }).ToList();
            }*/

        public List<List<AstroWeather.Helpers.Hour>> getWeatherinfo()
        {
            var weatherData = GetWeatherData();
            List<List<AstroWeather.Helpers.Hour>> listOfHoursPerDay = new List<List<AstroWeather.Helpers.Hour>>();
               if (weatherData != null)
            {
                
                listOfHoursPerDay = weatherData.days
     .Select(day =>
     {
         
         var dayDate = DateTime.ParseExact(day.datetime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
         var dateWithoutTime = dayDate.ToString("dd.MM.yyyy");
         
         foreach (var hour in day.hours)
         {
             hour.precip = hour.precip ?? 0;
             hour.cloudcover = hour.cloudcover ?? 0;
             hour.date = dateWithoutTime.ToString();
             hour.hour = hour.datetime.ToString().Substring(0, 2);
         }

         return day.hours.ToList(); // Zwróć listę godzin z dodaną datą
     })
     .ToList(); // Konwertujemy wynik na listę list
            }
            return listOfHoursPerDay;

            }
        public string[] getMooninfo()
        {
            string[] mooninfoarr = new string[3];
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            string formattedDate = today.ToString("yyyy-MM-dd");
        
            var moonInfoData = GetAstroData(formattedDate);
            mooninfoarr[0] = moonInfoData.moonrise.ToString();
            mooninfoarr[1] = moonInfoData.moonset.ToString();
            mooninfoarr[2] = moonInfoData.moon_illumination_percentage.ToString();
            mooninfoarr[3] = moonInfoData.moon_phase.ToString();
            return mooninfoarr;

        }
        private static string ReadResponseFromUrl(string url)
        {
            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(url)
            };
            using (var response = httpClient.GetAsync(url))
            {
                string responseBody = response.Result.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        
        
    }
}
