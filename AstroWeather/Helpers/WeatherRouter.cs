using System.ComponentModel.Design;
using System.Globalization;
using System.Text.Json;
//using Android.Health.Connect.DataTypes.Units;
using CosineKitty;
using NodaTime;
using NodaTime.Extensions;
using SolCalc;
using SolCalc.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace AstroWeather.Helpers
{
    public class WeatherRouter

    {
        private Astro? astronomicalData = null;
        private WeatherAPI? weatherData = null;
        static private double lat = 0;
        static private double lon = 0;



        private WeatherAPI GetWeatherData()
        {

            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();

            
            if (DefaultLatLon != null)
            {
                lat = DefaultLatLon[0];
                lon = DefaultLatLon[1];
                string APIkey = LogFileGetSet.GetAPIKey("weather");
                string LAT = lat.ToString().Replace(",", ".");
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
            else if (true)
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

        private static List<DateTime> getAstroTimes(DateTime date, bool first)
        {
            // Ustawienie strefy czasowej dla przekazanej daty
            var zone = DateTimeZoneProviders.Tzdb["Europe/Warsaw"];
            ZonedDateTime zonedDate = LocalDateTime.FromDateTime(date).InZoneLeniently(zone);

            // Obliczenie wczorajszego i dzisiejszego dnia w odpowiedniej strefie czasowej
            ZonedDateTime now = zonedDate;
            ZonedDateTime tommorow = now + Duration.FromDays(1);

            // Tworzenie obiektu reprezentującego Księżyc
            var moon = new AstroAlgo.SolarSystem.Moon(lat, lon, date, TimeZoneInfo.Local);

            // Pobieranie godzin zmierzchu i świtu
            var nauticalDuskChange = SunlightCalculator.GetSunlightChanges(now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.NauticalDusk);
            var astroDuskChange = SunlightCalculator.GetSunlightChanges(now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.AstronomicalDusk);

            var nauticalDawnChange = SunlightCalculator.GetSunlightChanges(first ? tommorow: now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.NauticalDawn);
            var astroDawnChange = SunlightCalculator.GetSunlightChanges(first ? tommorow : now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.AstronomicalDawn);

            // Formatowanie dat i godzin zmierzchu oraz świtu
            string nauticalDusk = nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");
            string nauticalDawn = nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");

            string astroDusk = astroDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + astroDuskChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");
            string astroDawn = astroDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + astroDawnChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");

            // Zaokrąglanie godzin zmierzchu i świtu
            string roundedDusk = roundHours(nauticalDusk, "DOWN");
            string roundedDawn = roundHours(nauticalDawn, "UP");

            // Obliczanie wschodu i zachodu Księżyca
            DateTime moonset = moon.DateTime + moon.Setting;
            DateTime moonrise = moon.DateTime + moon.Rising;

            // Lista do przechowywania wyników
            List<DateTime> astroTimes = new List<DateTime>();

            // Dodawanie czasów słońca i nocy
            if (!string.IsNullOrEmpty(roundedDusk)) astroTimes.Add(DateTime.ParseExact(roundedDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(roundedDawn)) astroTimes.Add(DateTime.ParseExact(roundedDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(astroDusk)) astroTimes.Add(DateTime.ParseExact(astroDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(astroDawn)) astroTimes.Add(DateTime.ParseExact(astroDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));

            // Dodawanie czasów Księżyca
            astroTimes.Add(moonset);
            astroTimes.Add(moonrise);

            return astroTimes;
        }




        public static List<List<AstroWeather.Helpers.Hour>> SetWeatherdata(List<AstroWeather.Helpers.Hour> inputList)
        {
            List<AstroWeather.Helpers.Hour> resultList = new List<AstroWeather.Helpers.Hour>();
            
            List<List<AstroWeather.Helpers.Hour>> outlist = new List<List<AstroWeather.Helpers.Hour>>();
            bool isNightOver = true;
            bool firstnight = true;
            List<DateTime> astroTimes = new List<DateTime>();
            foreach (var result in inputList)
            {
                string processedDate = result.date + " " + result.datetime;
                DateTime dateTime = DateTime.ParseExact(processedDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
              
                if (isNightOver || firstnight)
                {
                    astroTimes = getAstroTimes(dateTime, firstnight); 
                    isNightOver = false;
                    firstnight = false;

                }
                DateTime duskDateTime = astroTimes[0];  
                DateTime dawnDateTime = astroTimes[1];  
                
                if (DateTime.Compare(dateTime, duskDateTime) >= 0 && DateTime.Compare(dateTime, dawnDateTime) <= 0)
                {
                    resultList.Add(result);
                }
                else if (DateTime.Compare(dateTime, dawnDateTime) > 0) { isNightOver = true;


                    outlist.Add(new List<AstroWeather.Helpers.Hour>(resultList));
                    resultList.Clear();
                }
                
            }

            return outlist;
        }

        public static List<List<AstroWeather.Helpers.Hour>> CalculateWeatherdata(List<List<AstroWeather.Helpers.Hour>> inputList) {
            List<List<AstroWeather.Helpers.Hour>> calculatedWeatherList = new List<List<AstroWeather.Helpers.Hour>>();
            foreach (var result in inputList) {
                
                foreach (var hour in result)
                {
                    hour.riskOfDew = calculateRiskOfDew(hour);
                    hour.astroConditions = calculateAstroConditions(hour);
                }
            }
            var test = CalculateAstroNight(inputList);

                return inputList;
        }
        private static int calculateAstroConditions(AstroWeather.Helpers.Hour input) {
            int astro_night = 0;
            double ryzyko_mgly = input.riskOfDew ?? 100; 
            double precipitation = input.precip ?? 0;
            double cloud_cover = input.cloudcover ?? 100;
            double precipitation_probability = input.precipprob ;

            // Logika oceny warunków astronomicznych
            if (precipitation_probability < 50)
            {
                if (precipitation == 0 && ryzyko_mgly == 0)
                {
                    if (cloud_cover < 10)
                    {
                        astro_night = 10;
                    }
                }
                else if (precipitation == 0 && ryzyko_mgly < 20)
                {

                    if (cloud_cover < 10)
                    {
                        astro_night = 8;
                    }
                    else if (cloud_cover < 25)
                    {
                        astro_night = 6;
                    }
                }
                else if (precipitation < 0.1 && ryzyko_mgly < 50)
                {
                    if (cloud_cover < 40)
                    {
                        astro_night = 4;
                    }
                    else if (cloud_cover < 50)
                    {
                        astro_night = 2;
                    }
                }
            }

            return astro_night;
        }
        private static int calculateRiskOfDew(AstroWeather.Helpers.Hour input) {
            double ocena = 0;
            var relative_humidity = input.humidity;
            var visibility = input.visibility;
            var wind_speed = input.windspeed;
            var delta_temperature = Math.Abs(input.temp - input.dew);


            if (visibility <= 10) {
                ocena += 0.2;
                    if (relative_humidity >= 92) { ocena += 0.2; }

                if (delta_temperature <= 2) { ocena += 0.4; }

                else if (delta_temperature <= 4) { ocena += 0.2; }
                        
                    if (visibility <= 5 && wind_speed< 5) { ocena += 0.2; }
                        
                    }
            return Convert.ToInt32( Math.Round(ocena*1000));
        }

        private static List<double> CalculateAstroNight(List<List<AstroWeather.Helpers.Hour>> inputList)
        {
            List<double> calculatedAstroPerNight = new List<double>();
            foreach (var result in inputList)
            {
                double numberofnights = 0;
                double score = 0;
                foreach (var hour in result)
                {
                    numberofnights = result.Count()*10;
                    score += Convert.ToDouble( hour.astroConditions);
                }
                var score1 = score / numberofnights * 100;
                calculatedAstroPerNight.Add(Math.Round(score1));
            }

            return calculatedAstroPerNight;
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
