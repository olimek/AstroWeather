﻿namespace AstroWeather.Helpers
{
    public class Hour
    {
        public string? datetime { get; set; }
        public string? hour { get; set; }
        public string? date { get; set; }
        public int datetimeEpoch { get; set; }
        public double temp { get; set; }
        public double humidity { get; set; }
        public double dew { get; set; }
        public double? precip { get; set; }
        public double precipprob { get; set; }
        public double windspeed { get; set; }
        public double pressure { get; set; }
        public double visibility { get; set; }
        public double? cloudcover { get; set; }
        public int? riskOfDew { get; set; }
        public int? astroConditions { get; set; }
    }

    public class Day
    {
        public DateTime? ostAkt { get; set; }
        public string? datetime { get; set; }
        public int datetimeEpoch { get; set; }
        public double temp { get; set; }
        public double dew { get; set; }
        public double humidity { get; set; }
        public string? dayOfWeek { get; set; }
        public double precip { get; set; }
        public double precipprob { get; set; }
        public double windspeed { get; set; }
        public double pressure { get; set; }
        public double cloudcover { get; set; }
        public double visibility { get; set; }
        public double? astrocond { get; set; }
        public double? moonIlum { get; set; }
        public IList<Hour>? hours { get; set; }
        public List<DateTime>? AstroTimes { get; set; }
    }

    public class WeatherApi
    {
        public int queryCost { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string? resolvedAddress { get; set; }
        public string? address { get; set; }
        public string? timezone { get; set; }
        public double tzoffset { get; set; }

        public IList<Day>? days { get; set; }
    }
}