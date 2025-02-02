using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroWeather.Helpers;
namespace AstroWeather.Helpers
{
    public class DsoTrajectoryCalculator
    {
        public struct DsoPosition
        {
            public DateTime TimeUtc { get; set; }
            public double Altitude { get; set; }
            public double Azimuth { get; set; }
        }
      
        // Ustawienia lokalizacji – przykładowe wartości
        private const double ObserverLongitude = 17.0385; // przykładowa długość geograficzna
        private const double ObserverLatitude = 51.108;    // przykładowa szerokość geograficzna

        /// <summary>
        /// Oblicza trajektorię obiektu DSO między zachodem a wschodem słońca.
        /// </summary>
        /// <param name="ra">RA obiektu w godzinach</param>
        /// <param name="dec">DEC obiektu w stopniach</param>
        /// <param name="date">Data obliczeń (UTC)</param>
        /// <returns>Lista punktów trajektorii</returns>
        public static System.Collections.Generic.List<DsoPosition> CalculateTrajectory(double ra, double dec, DateTime date)
        {
            var trajectory = new System.Collections.Generic.List<DsoPosition>();
            
            // Przykładowe godziny zachodu i wschodu - w praktyce warto obliczyć te wartości!
            DateTime localSunset = date.Date.AddHours(18);
            DateTime localSunrise = date.Date.AddDays(1).AddHours(6);

            // Iteracja co 15 minut
            DateTime time = localSunset;
            while (time <= localSunrise)
            {
                double lst = CalculateLST(time);
                // RA jest podane w godzinach, dlatego mnożymy przez 15, by uzyskać stopnie.
                double ha = (lst - ra * 15) % 360;
                if (ha < 0)
                    ha += 360;

                double altitude = CalculateAltitude(ha, dec);
                double azimuth = CalculateAzimuth(ha, dec);

                // Uwzględniamy tylko punkty nad horyzontem
                if (altitude > 0)
                {
                    trajectory.Add(new DsoPosition
                    {
                        TimeUtc = time.ToUniversalTime(),
                        Altitude = altitude,
                        Azimuth = azimuth
                    });
                }

                time = time.AddMinutes(15);
            }

            return trajectory;
        }

        private static double CalculateLST(DateTime utcTime)
        {
            // Obliczenie liczby dni juliańskich
            double jd = (utcTime - new DateTime(2000, 1, 1, 12, 0, 0)).TotalDays + 2451545.0;
            double gst = (280.46061837 + 360.98564736629 * (jd - 2451545.0)) % 360;
            if (gst < 0)
                gst += 360;
            double lst = (gst + ObserverLongitude) % 360;
            if (lst < 0)
                lst += 360;
            return lst;
        }
        /// <summary>
        /// Konwertuje RA z formatu "hh mm" na godziny dziesiętne.
        /// </summary>
        static double ConvertRaToDecimal(string ra)
        {
            var parts = ra.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new FormatException("Nieprawidłowy format RA.");
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            return hours + minutes / 60.0;
        }

        /// <summary>
        /// Konwertuje DEC z formatu "+dd mm" lub "-dd mm" na stopnie dziesiętne.
        /// </summary>
        static double ConvertDecToDecimal(string dec)
        {
            var parts = dec.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new FormatException("Nieprawidłowy format DEC.");
            // Usuwamy znak +, lecz jeśli jest minus, metoda int.Parse już uwzględni znak
            int degrees = int.Parse(parts[0].Replace("+", ""));
            int minutes = int.Parse(parts[1]);
            // Jeśli znak minus występuje w pierwszym segmencie, wynik będzie ujemny
            return degrees + minutes / 60.0;
        }
        private static double CalculateAltitude(double ha, double dec)
        {
            double haRad = ha * Math.PI / 180;
            double decRad = dec * Math.PI / 180;
            double latRad = ObserverLatitude * Math.PI / 180;

            double sinAlt = Math.Sin(decRad) * Math.Sin(latRad) + Math.Cos(decRad) * Math.Cos(latRad) * Math.Cos(haRad);
            return Math.Asin(sinAlt) * 180 / Math.PI;
        }

        private static double CalculateAzimuth(double ha, double dec)
        {
            double haRad = ha * Math.PI / 180;
            double decRad = dec * Math.PI / 180;
            double latRad = ObserverLatitude * Math.PI / 180;

            // Obliczenie azymutu wg uproszczonego wzoru
            double cosAz = (Math.Sin(decRad) - Math.Sin(latRad) * Math.Sin(decRad)) / (Math.Cos(latRad) * Math.Cos(decRad));
            // Upewnienie się, że wartość mieści się w przedziale [-1,1]
            cosAz = Math.Max(-1, Math.Min(1, cosAz));
            double azimuth = Math.Acos(cosAz) * 180 / Math.PI;
            // Dostosowanie azymutu w zależności od kąta godzinowego
            if (Math.Sin(haRad) > 0)
                azimuth = 360 - azimuth;
            return azimuth;
        }
    }

}