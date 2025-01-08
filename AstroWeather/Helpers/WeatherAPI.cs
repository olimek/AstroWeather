using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroWeather.Helpers
{
    public class Hour
    {
        public string datetime { get; set; }
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
    }

    public class Day
    {
        public string datetime { get; set; }
        public int datetimeEpoch { get; set; }
        public double temp { get; set; }
        public double dew { get; set; }
        public double humidity { get; set; }
        public double precip { get; set; }
        public double precipprob { get; set; }
        public double windspeed { get; set; }
        public double pressure { get; set; }
        public double cloudcover { get; set; }
        public double visibility { get; set; }
        public IList<Hour> hours { get; set; }
    }

    public class WeatherAPI
    {
        public int queryCost { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string resolvedAddress { get; set; }
        public string address { get; set; }
        public string timezone { get; set; }
        public double tzoffset { get; set; }
        public IList<Day> days { get; set; }
    }


}
