using System.Globalization;
using System.Text.Json;
using NodaTime;
using NodaTime.Extensions;
using SolCalc;
using SolCalc.Data;
namespace AstroWeather.Helpers
{
    public class WeatherRouter

    {
        private WeatherAPI GetWeatherData()
        {

            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();
            if (DefaultLatLon != null)
            {
                string APIkey = LogFileGetSet.GetAPIKey("weather");
                string LAT = DefaultLatLon[0].Replace(",", ".");
                string LON = DefaultLatLon[1].Replace(",", ".");
                string jsonresponse = ReadResponseFromUrl($"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{LAT}%2C%20{LON}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Ctemp%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cwindspeed%2Cpressure%2Ccloudcover%2Cvisibility&include=days%2Chours%2Cfcst%2Cobs&key={APIkey}&contentType=json");

                jsonresponse = jsonresponse.Replace("null", "0");

                WeatherAPI? weather = JsonSerializer.Deserialize<WeatherAPI>(jsonresponse);

                return weather;
            }
            else { return null; }
        }

        private Astro GetAstroData(string DATE)
        {
            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();

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
        public static string GetWeatherImage()
        {
            double phase = 0;
            if (phase < 0.03 || phase > 0.97)
                return "sun.png";
            else if (phase < 0.22)
                return "night.png";
            else if (phase < 0.28)
                return "nightcloud.png";
            else if (phase < 0.47)
                return "nightrain.png";
            else if (phase < 0.53)
                return "cloudy.png";
            else if (phase < 0.72)
                return "cloudyrain.png";
            else
                return "heavyrain.png";

        }
        private static string roundHours(string hour, string round)
        {
            string outHour = "";

            if (TimeSpan.TryParse(hour, out TimeSpan parsedTime))
            {
                if (round == "UP")
                {
                    TimeSpan roundedUp = TimeSpan.FromHours(Math.Ceiling(parsedTime.TotalHours));
                    outHour = $"{roundedUp.Hours:00}:00:00";
                }
                else if (round == "DOWN")
                {
                    TimeSpan roundedDown = TimeSpan.FromHours(Math.Floor(parsedTime.TotalHours));
                    outHour = $"{roundedDown.Hours:00}:00:00";
                }
                else
                {
                    throw new ArgumentException("Invalid round value. Use 'UP' or 'DOWN'.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid hour format. Use 'HH:mm:ss'.");
            }

            return outHour;
        }

        public static List<AstroWeather.Helpers.Hour> SetWeatherdata(List<Hour> inputList)
        {
            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();


            foreach (var result in inputList)
            {
                string processeddate = result.date + " " + result.datetime;
                DateTime date = DateTime.ParseExact(processeddate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (date.Day >= DateTime.Now.Day && date.Day <= DateTime.Now.AddDays(1).Day)
                {


                    // Data do obliczeń

                    double lat = double.Parse(DefaultLatLon[0].Replace(",", "."), CultureInfo.InvariantCulture);
                    double lon = double.Parse(DefaultLatLon[1].Replace(",", "."), CultureInfo.InvariantCulture);
                    // Obliczenia dla Księżyca
                    var moon = new AstroAlgo.SolarSystem.Moon(lat, lon, date, TimeZoneInfo.Local);
                    var sun = new AstroAlgo.SolarSystem.Sun(lat, lon, date, TimeZoneInfo.Local);
                    ZonedDateTime now = SystemClock.Instance.InZone(DateTimeZoneProviders.Tzdb["Europe/Warsaw"]).GetCurrentZonedDateTime();
                    ZonedDateTime yesterday = now - Duration.FromDays(1);
                    var today = now.Date;
                    var currentSunlight = SunlightCalculator.GetSunlightChanges(yesterday, lat, lon)
    .First(change => change.Name == SolarTimeOfDay.NauticalDusk);
                    var currentSusssnlight = SunlightCalculator.GetSunlightChanges(yesterday, lat, lon)
    .First(change => change.Name == SolarTimeOfDay.NauticalDawn);

                    string duskTime = currentSunlight.Time.ToDateTimeUnspecified().ToString("HH:mm:ss");
                    string dawnTime = currentSusssnlight.Time.ToDateTimeUnspecified().ToString("HH:mm:ss");
                    // Przekazanie czasu jako string do funkcji roundHours

                    string ddd = roundHours(duskTime, "DOWN");
                    string ddssd = roundHours(dawnTime, "UP");

                    var testtt = moon.Setting.ToString();
                    var testt22t = moon.Rising.ToString();
                    // Wschód i zachód Księżyca


                    if (date.Hour != null) { }
                }

            }
            return null;

        }

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

         return day.hours.ToList();
     }).ToList();
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
