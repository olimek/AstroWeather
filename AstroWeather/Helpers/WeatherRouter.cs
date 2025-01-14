using System.Globalization;
using System.Text.Json;
using NodaTime;
using NodaTime.Extensions;
using SolCalc;
using SolCalc.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace AstroWeather.Helpers
{
    public class WeatherRouter

    {       private Astro? astronomicalData = null;
            private WeatherAPI? weatherData = null;
        static private double lat = 0;
        static private double lon = 0;



        private WeatherAPI GetWeatherData()
        {

            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();
            lat = DefaultLatLon[0];
            lon = DefaultLatLon[1];
            if (DefaultLatLon != null)
            {
                string APIkey = LogFileGetSet.GetAPIKey("weather");
                string LAT = lat.ToString().Replace(",",".");
                string LON = lon.ToString().Replace(",", ".");
                string jsonresponse = ReadResponseFromUrl($"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{LAT}%2C%20{LON}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Ctemp%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cwindspeed%2Cpressure%2Ccloudcover%2Cvisibility&include=days%2Chours%2Cfcst%2Cobs&key={APIkey}&contentType=json");

                jsonresponse = jsonresponse.Replace("null", "0");

                weatherData = JsonSerializer.Deserialize<WeatherAPI>(jsonresponse);

                return weatherData;
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
            string LAT = DefaultLatLon[0].ToString();
            string LON = DefaultLatLon[1].ToString();
            string jsonresponse = ReadResponseFromUrl($"https://api.ipgeolocation.io/astronomy?apiKey={APIkey}&lat={LAT}&long={LON}&date={DATE}");
            astronomicalData = JsonSerializer.Deserialize<Astro>(jsonresponse);
            return astronomicalData;
        }
        public static string GetWeatherImage()
        {
            double phase = 0;
            if (phase == 0)
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

        public static string GetMoonImage(double illumination)
        {
            
            if (illumination == 0)
                return "new_moon.png";
            else if (illumination > 0 && illumination < 50)
                return "waxing_crescent.png";
            else if (illumination == 50)
                return "first_quarter.png";
            else if (illumination > 50 && illumination < 100)
                return "waxing_gibbous.png";
            else if (illumination == 100)
                return "full_moon.png";
            /*else if (phase < 0.72)
                return "waning_gibbous.png";*/
            else if(true)
                return "last_quarter.png";
            else
                return "waning_crescent.png";

        }
        private static string roundHours(string dateTime, string round)
        {
            // Rozdzielamy datę i godzinę
            string[] parts = dateTime.Split(' ');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid format. Use 'HH:mm:ss DD:MM:YYYY'.");
            }

            string time = parts[0];
            string date = parts[1];

            // Parsowanie godziny z formatu HH:mm:ss
            if (TimeSpan.TryParse(time, out TimeSpan parsedTime))
            {
                // Zaokrąglanie godziny
                TimeSpan roundedTime;
                if (round == "UP")
                {
                    roundedTime = TimeSpan.FromHours(Math.Ceiling(parsedTime.TotalHours));
                }
                else if (round == "DOWN")
                {
                    roundedTime = TimeSpan.FromHours(Math.Floor(parsedTime.TotalHours));
                }
                else
                {
                    throw new ArgumentException("Invalid round value. Use 'UP' or 'DOWN'.");
                }

                // Tworzymy nowy DateTime z zaokrągloną godziną
                DateTime roundedDateTime = DateTime.ParseExact(date, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture)
                    .Add(roundedTime); // Dodajemy zaokrągloną godzinę do daty

                // Zwracamy zaokrągloną godzinę w formacie: HH:mm:ss dd:MM:yyyy
                return roundedDateTime.ToString("HH:mm:ss dd.MM.yyyy");
            }
            else
            {
                throw new ArgumentException("Invalid hour format. Use 'HH:mm:ss'.");
            }
        }

        private static List<DateTime> getAstroTimes(DateTime date)
        {
            // Utwórz obiekt reprezentujący Księżyc
            var moon = new AstroAlgo.SolarSystem.Moon(lat, lon, date, TimeZoneInfo.Local);

            // Ustawienie strefy czasowej
            ZonedDateTime now = SystemClock.Instance.InZone(DateTimeZoneProviders.Tzdb["Europe/Warsaw"]).GetCurrentZonedDateTime();
            ZonedDateTime yesterday = now - Duration.FromDays(1);

            // Pobranie czasu zmierzchu nautycznego
            var nauticalDuskChange = SunlightCalculator.GetSunlightChanges(yesterday, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.NauticalDusk);
            var astroDuskChange = SunlightCalculator.GetSunlightChanges(yesterday, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.AstronomicalDusk);

            // Pobranie czasu świtu nautycznego
            var nauticalDawnChange = SunlightCalculator.GetSunlightChanges(now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.NauticalDawn);

            var astroDawnChange = SunlightCalculator.GetSunlightChanges(now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.AstronomicalDawn);

            // Zaokrąglanie godzin zmierzchu i świtu
            string nauticalDusk = nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");
            string nauticalDawn = nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");

            string astroDusk = astroDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + astroDuskChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");
            string astroDawn = astroDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + astroDawnChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");

            string roundedDusk = roundHours(nauticalDusk, "DOWN");
            string roundedDawn = roundHours(nauticalDawn, "UP");

            

            // Konwersja TimeSpan na DateTime dla wschodu i zachodu Księżyca
            DateTime moonset =  moon.DateTime + moon.Setting;
            DateTime moonrise = moon.DateTime + moon.Rising;

            // Dodanie wartości do listy
            List<DateTime> astroTimes = new List<DateTime>();

            // Dodanie czasów słońca i nocy
            if (!string.IsNullOrEmpty(roundedDusk)) astroTimes.Add(DateTime.Parse(roundedDusk));
            if (!string.IsNullOrEmpty(roundedDawn)) astroTimes.Add(DateTime.Parse(roundedDawn));
            if (!string.IsNullOrEmpty(astroDusk)) astroTimes.Add(DateTime.Parse(astroDusk));
            if (!string.IsNullOrEmpty(astroDawn)) astroTimes.Add(DateTime.Parse(astroDawn));

            // Dodanie czasów Księżyca
            astroTimes.Add(moonset);
            astroTimes.Add(moonrise);

            return astroTimes;
        }




        public static List<AstroWeather.Helpers.Hour> SetWeatherdata(List<Hour> inputList)
        {

            List<string> test = new List<string>();

            foreach (var result in inputList)
            {
                string processeddate = result.date + " " + result.datetime;
                DateTime date = DateTime.ParseExact(processeddate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (date.Day >= DateTime.Now.Day && date.Day <= DateTime.Now.AddDays(1).Day)
                {
                    var czasy = getAstroTimes(date);

                    // Data do obliczeń

                    // Wschód i zachód Księżyca
                    

                    if (date.Hour > czasy[0].Hour) {
                        test.Add(date.Hour.ToString());
                    }
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
