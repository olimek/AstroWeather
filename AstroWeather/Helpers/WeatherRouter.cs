using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AstroWeather.Helpers;
using System.Text.Json;

namespace AstroWeather.Helpers
{
    public class WeatherRouter

    {
        public WeatherAPI WeatherInit()
        {
            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();
            string APIkey = LogFileGetSet.getAPIkey("weather");
            string LAT = DefaultLatLon[0].Replace(",", ".");
            string LON = DefaultLatLon[1].Replace(",", ".");
            string jsonresponse = MainPage.ReadResponseFromUrl($"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{LAT}%2C%20{LON}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Ctemp%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cwindspeed%2Cpressure%2Ccloudcover%2Cvisibility&include=days%2Chours%2Cfcst%2Cobs&key={APIkey}&contentType=json");
            WeatherAPI? weather = JsonSerializer.Deserialize<WeatherAPI>(jsonresponse);
            return weather;
        }
    }
}
