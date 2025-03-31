using System.Globalization;
using System.Text.Json;
using CosineKitty;
using NodaTime;
using SkiaSharp;
using SolCalc;
using SolCalc.Data;

namespace AstroWeather.Helpers
{
    public class WeatherRouter
    {
        private static WeatherApi? weatherData = null;
        private static double lat;
        private static double lon;

        public static double Lat => lat;
        public static double Lon => lon;

        private static async Task<WeatherApi?> GetWeatherDataAsync()
        {
            var defaultLatLon = await LogFileGetSet.LoadDefaultLocAsync();
            if (defaultLatLon != null && defaultLatLon.Count > 1)
            {
                lat = defaultLatLon[0];
                lon = defaultLatLon[1];
                string? apiKey = await LogFileGetSet.GetAPIKeyAsync();
                string latStr = lat.ToString(CultureInfo.InvariantCulture);
                string lonStr = lon.ToString(CultureInfo.InvariantCulture);

                string jsonResponse = await WeatherRouter.ReadResponseFromUrlAsync($"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{latStr}%2C{lonStr}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Ctemp%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cwindspeed%2Cpressure%2Ccloudcover%2Cvisibility&include=days%2Chours%2Cfcst%2Cobs&key={apiKey}&contentType=json");
                jsonResponse = jsonResponse.Replace("null", "0");

                weatherData = JsonSerializer.Deserialize<WeatherApi>(jsonResponse);
                return weatherData;
            }
            else
            {
                return null;
            }
        }

        public static bool IsApiVariables()
        {
            var isAPI = LogFileGetSet.GetAPIKeyAsync();
            var isDefLoc = LogFileGetSet.LoadDefaultLocAsync();
            if (isAPI == null || isDefLoc == null) return false;
            else return true;
        }

        public static async Task<List<Day>>? SetWeatherBindingContextAsync()
        {
            var weatherInfo = await WeatherRouter.GetWeatherInfoAsync();
            var result2 = weatherInfo.SelectMany(i => i).Distinct();
            var ss = SetWeatherData(result2.ToList());
            CalculateWeatherData(ss);
            CalculateAstroNight(ss);

            var dailyData = await WeatherRouter.GetCalculatedDailyAsync(ss);
            if (dailyData != null)
            {
                return dailyData;
            }
            return new List<Day>();
        }

        public async Task<List<DayWithHours>?> GetCarouselViewAsync()
        {
            var weatherInfo = await WeatherRouter.GetWeatherInfoAsync();
            var result2 = weatherInfo.SelectMany(i => i).Distinct();
            var ss = SetWeatherData(result2.ToList());
            CalculateWeatherData(ss);
            CalculateAstroNight(ss);

            var dailyData = await WeatherRouter.GetCalculatedDailyAsync(ss);

            if (ss.Count != 0)
            {
                var daysWithHours = new List<DayWithHours>();

                for (int i = 0; i < ss.Count; i++)
                {
                    DateTime currentdate = DateTime.ParseExact(ss[i][0].date!, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                    var time = new AstroTime(currentdate);
                    IllumInfo illum = Astronomy.Illumination(Body.Moon, time);
                    var astroTimes = GetAstroTimes(currentdate, true);

                    var day = new DayWithHours
                    {
                        moonrise = astroTimes[5].ToString("dd.MM HH:mm"),
                        moonset = astroTimes[4].ToString("dd.MM HH:mm"),
                        nauticalstart = astroTimes[6].ToString("dd.MM HH:mm"),
                        nauticalend = astroTimes[7].ToString("dd.MM HH:mm"),
                        astrostart = astroTimes[2].ToString("dd.MM HH:mm"),
                        astroend = astroTimes[3].ToString("dd.MM HH:mm"),
                        moonilum = Math.Round(100.0 * illum.phase_fraction).ToString(),
                        condition = dailyData[i].astrocond?.ToString() ?? "0",
                        DayOfWeek = GetPolishDayOfWeek(currentdate),
                        Date = currentdate.ToString("dd.MM"),
                        Hours = ss[i]
                    };

                    daysWithHours.Add(day);
                }
                return daysWithHours;
            }

            return null;
        }

        private static string GetPolishDayOfWeek(DateTime date)
        {
            string[] dniTygodnia = { "Niedziela", "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek", "Sobota" };
            return dniTygodnia[(int)date.DayOfWeek];
        }

        public static string GetWeatherImage(double phase)
        {
            if (phase <= 0)
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

        public static string GetMoonImage(double moonIllumination, double phaseAngle)
        {
            moonIllumination = Math.Clamp(moonIllumination, 0, 100);
            phaseAngle = phaseAngle % 360;

            if (moonIllumination <= 10)
                return "new_moon.png";
            else if (moonIllumination >= 90)
                return "full_moon.png";

            if (phaseAngle >= 0 && phaseAngle < 180)
            {
                if (moonIllumination >= 10 && moonIllumination < 40)
                    return "waxing_crescent.png";
                else if (moonIllumination >= 40 && moonIllumination < 60)
                    return "first_quarter.png";
                else if (moonIllumination >= 60 && moonIllumination < 90)
                    return "waxing_gibbous.png";
            }
            else if (phaseAngle >= 180 && phaseAngle < 360)
            {
                if (moonIllumination >= 60 && moonIllumination < 90)
                    return "waning_gibbous.png";
                else if (moonIllumination >= 40 && moonIllumination < 60)
                    return "last_quarter.png";
                else if (moonIllumination >= 10 && moonIllumination < 40)
                    return "waning_crescent.png";
            }

            return "new_moon.png";
        }

        private static string RoundHours(string dateTime, string round)
        {
            string[] parts = dateTime.Split(' ');
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid format. Use 'HH:mm:ss DD:MM:YYYY'.");
            }

            string time = parts[0];
            string date = parts[1];

            if (TimeSpan.TryParse(time, CultureInfo.InvariantCulture, out TimeSpan parsedTime))
            {
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

                DateTime roundedDateTime = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture).Add(roundedTime);
                return roundedDateTime.ToString("HH:mm:ss dd.MM.yyyy");
            }
            else
            {
                throw new ArgumentException("Invalid hour format. Use 'HH:mm:ss'.");
            }
        }

        public static List<DateTime> GetAstroTimes(DateTime date, bool first)
        {
            var zone = DateTimeZoneProviders.Tzdb["Europe/Warsaw"];
            ZonedDateTime zonedDate = LocalDateTime.FromDateTime(date).InZoneLeniently(zone);
            ZonedDateTime now = zonedDate;
            ZonedDateTime tomorrow = now + Duration.FromDays(1);

            var moon = new AstroAlgo.SolarSystem.Moon(lat, lon, date, TimeZoneInfo.Local);

            var nauticalDuskChange = SunlightCalculator.GetSunlightChanges(now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.NauticalDusk);
            var astroDuskChange = SunlightCalculator.GetSunlightChanges(now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.AstronomicalDusk);

            var nauticalDawnChange = SunlightCalculator.GetSunlightChanges(first ? tomorrow : now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.NauticalDawn);
            var astroDawnChange = SunlightCalculator.GetSunlightChanges(first ? tomorrow : now, lat, lon)
                .FirstOrDefault(change => change.Name == SolarTimeOfDay.AstronomicalDawn);

            string nauticalDusk = nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss dd.MM.yyyy");
            string nauticalDawn = nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss dd.MM.yyyy");
            string astroDusk = astroDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss dd.MM.yyyy");
            string astroDawn = astroDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss dd.MM.yyyy");

            string roundedDusk = RoundHours(nauticalDusk, "DOWN");
            string roundedDawn = RoundHours(nauticalDawn, "UP");

            var baseDate = date.Date;
            DateTime? moonrise = null;
            DateTime? moonset = null;

            if (!double.IsNaN(moon.Rising.TotalMinutes) && moon.Rising != TimeSpan.Zero)
            {
                moonrise = baseDate.Add(moon.Rising);
                // Jeśli po północy (czyli np. 02:00), dodaj 1 dzień
                if (moon.Rising.TotalHours < 12)
                    moonrise = moonrise.Value.AddDays(1);
            }

            if (!double.IsNaN(moon.Setting.TotalMinutes) && moon.Setting != TimeSpan.Zero)
                moonset = baseDate.Add(moon.Setting);

            // Jeśli zachód jest wcześniej niż wschód → przesuwamy zachód
            if (moonrise.HasValue && moonset.HasValue && moonset < moonrise)
                moonset = moonset.Value.AddDays(1);

            List<DateTime> astroTimes = new();

            if (!string.IsNullOrEmpty(roundedDusk)) astroTimes.Add(DateTime.ParseExact(roundedDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(roundedDawn)) astroTimes.Add(DateTime.ParseExact(roundedDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(astroDusk)) astroTimes.Add(DateTime.ParseExact(astroDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(astroDawn)) astroTimes.Add(DateTime.ParseExact(astroDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (moonset.HasValue) astroTimes.Add(moonset.Value);
            if (moonrise.HasValue) astroTimes.Add(moonrise.Value);
            if (!string.IsNullOrEmpty(nauticalDusk)) astroTimes.Add(DateTime.ParseExact(nauticalDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(nauticalDawn)) astroTimes.Add(DateTime.ParseExact(nauticalDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));

            return astroTimes;
        }

        public static void DrawNightTimeline(
    SKCanvas canvas, SKImageInfo info, DateTime date, bool first, int moonIllumination)
        {
            var ev = GetAstroTimes(date, first);
            if (ev.Count != 8) return;

            DateTime nauticalStartRounded = ev[0];
            DateTime nauticalEndRounded = ev[1];
            DateTime astroDawn = ev[2];
            DateTime astroDusk = ev[3];
            DateTime moonSet = ev[4];
            DateTime moonRise = ev[5];
            DateTime nauticalEnd = ev[6];
            DateTime nauticalStart = ev[7];

            DateTime start = nauticalStartRounded.AddHours(-1);
            DateTime end = nauticalEndRounded.AddHours(1);
            double totalMin = (end - start).TotalMinutes;

            float margin = info.Width * 0.05f;
            float usableWidth = info.Width - 2 * margin;

            float X(DateTime dt) => margin + (float)((dt - start).TotalMinutes / totalMin * usableWidth);

            canvas.Clear(SKColors.Transparent);

            float barY = info.Height * 0.3f;
            float barH = info.Height * 0.25f;

            using var gradPaint = new SKPaint
            {
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(margin, 0),
                    new SKPoint(margin + usableWidth, 0),
                    new[]
                    {
                SKColors.LightSkyBlue,
                SKColors.MidnightBlue,
                SKColors.Black,
                SKColors.MidnightBlue,
                SKColors.LightSkyBlue
                    },
                    new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f },
                    SKShaderTileMode.Clamp
                )
            };
            canvas.DrawRect(margin, barY, usableWidth, barH, gradPaint);

            // Moonlight
            DateTime lower = (moonRise < moonSet) ? moonRise : moonSet;
            DateTime upper = (moonRise < moonSet) ? moonSet : moonRise;

            var clippedLow = (lower < start) ? start : lower;
            var clippedUp = (upper > end) ? end : upper;

            if (clippedUp > clippedLow)
            {
                byte alpha = (byte)(255 * Math.Clamp(moonIllumination / 100.0, 0, 1));
                var moonColor = new SKColor(180, 180, 255, alpha);
                using var moonPaint = new SKPaint { Color = moonColor };
                canvas.DrawRect(X(clippedLow), barY, X(clippedUp) - X(clippedLow), barH, moonPaint);
            }

            var markerPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 2 };
            var labelPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = info.Height * 0.06f,
                IsAntialias = true
            };

            void Mark(DateTime dt, string label)
            {
                if (dt < start || dt > end) return;
                float x = X(dt);
                canvas.DrawLine(x, barY, x, barY + barH, markerPaint);
                canvas.DrawText(label, x + 2, barY - 8, labelPaint);
            }

            Mark(nauticalStartRounded, "Naut");
            Mark(astroDusk, "Astro");
            Mark(astroDawn, "Astro");
            Mark(nauticalEndRounded, "Naut");

            // ➕ Moon markers
            Mark(moonRise, "Moon");
            Mark(moonSet, "Moon");
        }




        public static List<List<Hour>> SetWeatherData(List<Hour> inputList)
        {
            List<Hour> resultList = new List<Hour>();
            List<List<Hour>> outList = new List<List<Hour>>();
            bool isNightOver = true;
            bool firstNight = true;
            List<DateTime> astroTimes = new List<DateTime>();

            foreach (var result in inputList)
            {
                string processedDate = result.date + " " + result.datetime;
                DateTime dateTime = DateTime.ParseExact(processedDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                if (isNightOver || firstNight)
                {
                    astroTimes = GetAstroTimes(dateTime, firstNight);
                    isNightOver = false;
                    firstNight = false;
                }

                DateTime duskDateTime = astroTimes[0];
                DateTime dawnDateTime = astroTimes[1];

                if (DateTime.Compare(dateTime, duskDateTime) >= 0 && DateTime.Compare(dateTime, dawnDateTime) <= 0)
                {
                    resultList.Add(result);
                }
                else if (DateTime.Compare(dateTime, dawnDateTime) > 0)
                {
                    isNightOver = true;
                    outList.Add(new List<Hour>(resultList));
                    resultList.Clear();
                }
            }

            return outList;
        }

        public static List<List<Hour>> CalculateWeatherData(List<List<Hour>> inputList)
        {
            foreach (var result in inputList)
            {
                foreach (var hour in result)
                {
                    hour.riskOfDew = CalculateRiskOfDew(hour);
                    hour.astroConditions = CalculateAstroConditions(hour);
                }
            }
            return inputList;
        }

        private static int CalculateAstroConditions(Hour input)
        {
            int astroNight = 0;
            double riskOfDew = input.riskOfDew ?? 100;
            double precipitation = input.precip ?? 0;
            double cloudCover = input.cloudcover ?? 100;
            double precipitationProbability = input.precipprob;

            if (precipitationProbability < 50)
            {
                if (precipitation <= 0 && riskOfDew <= 0)
                {
                    if (cloudCover < 10)
                    {
                        astroNight = 10;
                    }
                }
                else if (precipitation <= 0 && riskOfDew < 20)
                {
                    if (cloudCover < 10)
                    {
                        astroNight = 8;
                    }
                    else if (cloudCover < 25)
                    {
                        astroNight = 6;
                    }
                }
                else if (precipitation < 0.1 && riskOfDew < 50)
                {
                    if (cloudCover < 40)
                    {
                        astroNight = 4;
                    }
                    else if (cloudCover < 50)
                    {
                        astroNight = 2;
                    }
                }
            }

            return astroNight;
        }

        private static int CalculateRiskOfDew(Hour input)
        {
            double ocena = 0;
            var relativeHumidity = input.humidity;
            var visibility = input.visibility;
            var windSpeed = input.windspeed;
            var deltaTemperature = Math.Abs(input.temp - input.dew);

            if (visibility <= 10)
            {
                ocena += 0.2;
                if (relativeHumidity >= 92)
                {
                    ocena += 0.2;
                }

                if (deltaTemperature <= 2)
                {
                    ocena += 0.4;
                }
                else if (deltaTemperature <= 4)
                {
                    ocena += 0.2;
                }

                if (visibility <= 5 && windSpeed < 5)
                {
                    ocena += 0.2;
                }
            }

            return Convert.ToInt32(Math.Round(ocena * 100));
        }

        public static Task<List<Day>> GetCalculatedDailyAsync(List<List<Hour>> inputList)
        {
            var dailyOut = new List<Day>();
            CalculateAstroNight(inputList);
            var weatherDataDays = weatherData?.days ?? new List<Day>();
            int i = 0;

            foreach (var day in weatherDataDays)
            {
                DateTime dateTime = DateTime.ParseExact(day.datetime!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var time = new AstroTime(dateTime);
                List<DateTime> astroTimes = GetAstroTimes(dateTime, true);
                IllumInfo illum = Astronomy.Illumination(Body.Moon, time);
                day.dayOfWeek = GetPolishDayOfWeek(dateTime);
                day.moonIlum = Math.Round(100.0 * illum.phase_fraction);
                day.AstroTimes = astroTimes;
                dailyOut.Add(day);
            }

            foreach (var result in inputList)
            {
                double numberOfNights = 0;
                double score = 0;
                foreach (var hour in result)
                {
                    numberOfNights = result.Count * 10;
                    score += Convert.ToDouble(hour.astroConditions);
                }

                dailyOut[i].astrocond = Math.Round(score / numberOfNights * 100);
                i++;
            }

            return Task.FromResult(dailyOut);
        }

        public static List<double> CalculateAstroNight(List<List<Hour>> inputList)
        {
            List<double> calculatedAstroPerNight = new List<double>();
            foreach (var result in inputList)
            {
                double numberOfNights = 0;
                double score = 0;
                foreach (var hour in result)
                {
                    numberOfNights = result.Count * 10;
                    score += Convert.ToDouble(hour.astroConditions);
                }
                var score1 = score / numberOfNights * 100;
                calculatedAstroPerNight.Add(Math.Round(score1));
            }

            return calculatedAstroPerNight;
        }

        public static async Task<List<List<Hour>>> GetWeatherInfoAsync()
        {
            var weatherDataa = await WeatherRouter.GetWeatherDataAsync();
            var listOfHoursPerDay = new List<List<Hour>>();

            if (weatherDataa != null)
            {
                listOfHoursPerDay = weatherDataa.days!
                    .Select(day =>
                    {
                        var dayDate = DateTime.ParseExact(day.datetime!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        var dateWithoutTime = dayDate.ToString("dd.MM.yyyy");

                        foreach (var hour in day.hours!)
                        {
                            hour.precip = hour.precip ?? 0;
                            hour.cloudcover = hour.cloudcover ?? 0;
                            hour.date = dateWithoutTime;
                            hour.hour = hour.datetime!.ToString().Substring(0, 2);
                        }

                        return day.hours.ToList();
                    }).ToList();
            }
            return listOfHoursPerDay;
        }

        private static async Task<string> ReadResponseFromUrlAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}