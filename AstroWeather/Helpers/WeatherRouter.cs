﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CosineKitty;
using Innovative.Geometry;
using NodaTime;
using NodaTime.Extensions;
using SolCalc;
using SolCalc.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AstroWeather.Helpers
{
    public class WeatherRouter
    {
        
        private static WeatherAPI? weatherData = null;
        public static double lat = 0;
        public static double lon = 0;
        private readonly LogFileGetSet logFileGetSet = new LogFileGetSet();

        private async Task<WeatherAPI?> GetWeatherDataAsync()
        {
            var defaultLatLon = await logFileGetSet.LoadDefaultLocAsync();
            if (defaultLatLon != null && defaultLatLon.Count() > 1)
            {
                lat = defaultLatLon[0];
                lon = defaultLatLon[1];
                string apiKey = await logFileGetSet.GetAPIKeyAsync("weather");
                string latStr = lat.ToString(CultureInfo.InvariantCulture);
                string lonStr = lon.ToString(CultureInfo.InvariantCulture);

                string jsonResponse = await ReadResponseFromUrlAsync($"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{latStr}%2C{lonStr}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Ctemp%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cwindspeed%2Cpressure%2Ccloudcover%2Cvisibility&include=days%2Chours%2Cfcst%2Cobs&key={apiKey}&contentType=json");
                jsonResponse = jsonResponse.Replace("null", "0");

                weatherData = JsonSerializer.Deserialize<WeatherAPI>(jsonResponse);
                return weatherData;
            }
            else
            {
                return null;
            }
        }
        public static async Task<List<Day>> SetWeatherBindingContextAsync()
        {
            var daysWithHours = new List<DayWithHours>();
            var weatherRouter = new WeatherRouter();
            var weatherInfo = await weatherRouter.GetWeatherInfoAsync();
            var result2 = weatherInfo.SelectMany(i => i).Distinct();
            var ss = SetWeatherData(result2.ToList());
            var hourlyConditions = CalculateWeatherData(ss);
            var dailyConditions = CalculateAstroNight(ss);
            var dailyData = await weatherRouter.GetCalculatedDailyAsync(ss);
            if (dailyData != null){ 
                return dailyData;
            }
            return null;
        }
        
        public static async Task<List<DayWithHours>?> GetCarouselViewAsync()
        {
            var daysWithHours = new List<DayWithHours>();
            var weatherRouter = new WeatherRouter();
            var weatherInfo = await weatherRouter.GetWeatherInfoAsync();
            var result2 = weatherInfo.SelectMany(i => i).Distinct();
            var ss = SetWeatherData(result2.ToList());
            var hourlyConditions = CalculateWeatherData(ss);
            var dailyConditions = CalculateAstroNight(ss);
            var dailyData = await weatherRouter.GetCalculatedDailyAsync(ss);
            
            if (ss.Count != 0)
            {
                

                for (int i = 0; i < ss.Count; i++)
                {
                    DateTime currentdate = DateTime.ParseExact(ss[i][0].date, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    
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
                        condition = dailyData[i].astrocond.ToString(),
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

        public static string GetMoonImage(double moonIllumination, double phaseAngle)
        {
            moonIllumination = Math.Clamp(moonIllumination, 0, 100); // Ensure illumination is between 0-100
            phaseAngle = phaseAngle % 360; // Ensure angle is between 0-360

            if (moonIllumination <= 10)
                return "new_moon.png";  // New Moon
            else if (moonIllumination >= 90)
                return "full_moon.png";  // Full Moon

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

            return "new_moon.png"; // Default case
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

            if (TimeSpan.TryParse(time, out TimeSpan parsedTime))
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

            string nauticalDusk = nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + nauticalDuskChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");
            string nauticalDawn = nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + nauticalDawnChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");

            string astroDusk = astroDuskChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + astroDuskChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");
            string astroDawn = astroDawnChange.Time.ToDateTimeUnspecified().ToString("HH:mm:ss") + " " + astroDawnChange.Time.ToDateTimeUnspecified().ToString("dd.MM.yyyy");

            string roundedDusk = RoundHours(nauticalDusk, "DOWN");
            string roundedDawn = RoundHours(nauticalDawn, "UP");

            DateTime moonset = moon.DateTime + moon.Setting;
            DateTime moonrise = moon.DateTime + moon.Rising;

            List<DateTime> astroTimes = new List<DateTime>();

            if (!string.IsNullOrEmpty(roundedDusk)) astroTimes.Add(DateTime.ParseExact(roundedDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(roundedDawn)) astroTimes.Add(DateTime.ParseExact(roundedDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(astroDusk)) astroTimes.Add(DateTime.ParseExact(astroDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(astroDawn)) astroTimes.Add(DateTime.ParseExact(astroDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));

            astroTimes.Add(moonset);
            astroTimes.Add(moonrise);
            if (!string.IsNullOrEmpty(nauticalDusk)) astroTimes.Add(DateTime.ParseExact(nauticalDusk, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(nauticalDawn)) astroTimes.Add(DateTime.ParseExact(nauticalDawn, "HH:mm:ss dd.MM.yyyy", CultureInfo.InvariantCulture));
            return astroTimes;
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
                if (precipitation == 0 && riskOfDew == 0)
                {
                    if (cloudCover < 10)
                    {
                        astroNight = 10;
                    }
                }
                else if (precipitation == 0 && riskOfDew < 20)
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

        public async Task<List<Day>> GetCalculatedDailyAsync(List<List<Hour>> inputList)
        {
            var dailyOut = new List<Day>();
            var astronight = CalculateAstroNight(inputList);
            var weatherDataDays = weatherData?.days ?? new List<Day>();
            int i = 0;

            foreach (var day in weatherDataDays)
            {
                DateTime dateTime = DateTime.ParseExact(day.datetime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
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
                    numberOfNights = result.Count() * 10;
                    score += Convert.ToDouble(hour.astroConditions);
                }

                dailyOut[i].astrocond = Math.Round(score / numberOfNights * 100);
                i++;
            }

            return dailyOut;
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
                    numberOfNights = result.Count() * 10;
                    score += Convert.ToDouble(hour.astroConditions);
                }
                var score1 = score / numberOfNights * 100;
                calculatedAstroPerNight.Add(Math.Round(score1));
            }

            return calculatedAstroPerNight;
        }

        public async Task<List<List<Hour>>> GetWeatherInfoAsync()
        {
            var weatherData = await GetWeatherDataAsync();
            var listOfHoursPerDay = new List<List<Hour>>();

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
                            hour.date = dateWithoutTime;
                            hour.hour = hour.datetime.ToString().Substring(0, 2);
                        }

                        return day.hours.ToList();
                    }).ToList();
            }
            return listOfHoursPerDay;
        }
        
        private async Task<string> ReadResponseFromUrlAsync(string url)
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
