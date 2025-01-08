using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroWeather.Helpers
{
    public class Location
    {
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    public class Astro
    {
        public Location location { get; set; }
        public string date { get; set; }
        public string current_time { get; set; }
        public string sunrise { get; set; }
        public string sunset { get; set; }
        public string sun_status { get; set; }
        public string solar_noon { get; set; }
        public string day_length { get; set; }
        public double sun_altitude { get; set; }
        public double sun_distance { get; set; }
        public double sun_azimuth { get; set; }
        public string moonrise { get; set; }
        public string moonset { get; set; }
        public string moon_status { get; set; }
        public double moon_altitude { get; set; }
        public double moon_distance { get; set; }
        public double moon_azimuth { get; set; }
        public double moon_parallactic_angle { get; set; }
        public string moon_phase { get; set; }
        public string moon_illumination_percentage { get; set; }
        public double moon_angle { get; set; }
    }


}
