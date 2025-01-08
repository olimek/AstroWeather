﻿

using System.Text.Json;
using AstroWeather.Helpers;
using System.Net.Http;
using static System.Net.Mime.MediaTypeNames;
namespace AstroWeather
{

    public partial class MainPage : ContentPage
    {
        int count = 0;
        public MainPage()
        {
            InitializeComponent();
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            string formattedDate = today.ToString("yyyy-MM-dd");
            var test = GetAstroData(formattedDate);
            moonrisel.Text = test.moonrise;
            moonsetl.Text = test.moonset;
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
            }
        }
        

        

        public static string ReadResponseFromUrl(string url)
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

        private Astro GetAstroData(string DATE)
        {
            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();

            // Sprawdzenie poprawności danych lokalizacji
            if (DefaultLatLon == null || DefaultLatLon.Count < 2)
            {
                throw new Exception("DefaultLatLon is null or incomplete.");
            }
            string APIkey = LogFileGetSet.getAPIkey("astro");
            string LAT = DefaultLatLon[0].Replace(",", ".");
            string LON = DefaultLatLon[1].Replace(",", ".");
            string jsonresponse = ReadResponseFromUrl($"https://api.ipgeolocation.io/astronomy?apiKey={APIkey}&lat={LAT}&long={LON}&date={DATE}");
            Astro? astro = JsonSerializer.Deserialize<Astro>(jsonresponse);
            return astro;
        }
    }
}
