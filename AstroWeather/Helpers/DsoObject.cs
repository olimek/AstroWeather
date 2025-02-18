using System;
using System.Collections.Generic;
using System.IO;
using CosineKitty;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace AstroWeather
{
    public class DsoObject
    {
        public string? Constellation { get; set; }
        public string? Dec { get; set; }
        public string? Description { get; set; }
        public double? Mag { get; set; }
        public string? Name { get; set; }
        public string? Ra { get; set; }
        public double? Size { get; set; }
        public string? Type { get; set; }
        public double MaxAlt { get; set; }
        public double Visible { get; set; }
        public double SumAlitude { get; set; }
        public bool photo { get; set; }
    }
}
namespace AstroWeather.Helpers
{
    public class DsoCalculator
    {
        private readonly List<AstroWeather.DsoObject> _dsoObjects;
        private static async Task<string?> ReadYamlFileAsync(string fileName)
        {

            var streamhh = await FileSystem.AppPackageFileExistsAsync(fileName);
            if (streamhh)
            {
                using Stream stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                return content;
            }
            else { return null; }
        }
        private DsoCalculator(List<AstroWeather.DsoObject> dsoObjects)
        {
            _dsoObjects = dsoObjects;
        }

        public static async Task<DsoCalculator> CreateAsync(string fileName)
        {
            string? yamlText = await ReadYamlFileAsync(fileName);

            if (yamlText == null)
            {
                throw new FileNotFoundException($"The file {fileName} could not be found or read.");
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            List<AstroWeather.DsoObject> dsoObjects = deserializer.Deserialize<List<AstroWeather.DsoObject>>(yamlText);

            return new DsoCalculator(dsoObjects);
        }

        public static async Task UpdateYamlFileAsync(string fileName, Action<List<AstroWeather.DsoObject>> updateAction)
        {
            string? yamlText = await ReadYamlFileAsync(fileName);
            if (yamlText == null)
            {
                throw new FileNotFoundException($"The file {fileName} could not be found or read.");
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            List<AstroWeather.DsoObject> dsoObjects = deserializer.Deserialize<List<AstroWeather.DsoObject>>(yamlText);

            updateAction(dsoObjects);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string updatedYamlText = serializer.Serialize(dsoObjects);

            await File.WriteAllTextAsync(fileName, updatedYamlText);
        }

        public static double ParseRA(string raStr)
        {
            if (string.IsNullOrEmpty(raStr))
                throw new ArgumentNullException(nameof(raStr), "RA string cannot be null or empty.");

            var parts = raStr.Split(' ');
            if (parts.Length < 2 || parts.Length > 3)
                throw new FormatException("RA should be in the format HH MM, HH MM SS, or HH MM.m");

            double hours = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
            double minutes = 0.0;
            double seconds = 0.0;

            if (parts.Length >= 2)
                minutes = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

            if (parts.Length == 3)
                seconds = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

            return hours + (minutes / 60.0) + (seconds / 3600.0);
        }

        public static double ParseDec(string decStr)
        {
            if (string.IsNullOrEmpty(decStr))
                throw new ArgumentNullException(nameof(decStr), "Dec string cannot be null or empty.");

            var parts = decStr.Split(' ');
            if (parts.Length < 2 || parts.Length > 3)
                throw new FormatException("Dec should be in the format ±DD MM, ±DD MM SS, or ±DD MM.m");

            int degrees = int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
            double minutes = 0.0;
            double seconds = 0.0;

            if (parts.Length >= 2)
                minutes = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

            if (parts.Length == 3)
                seconds = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

            double sign = degrees < 0 ? -1 : 1;
            double absDegrees = Math.Abs(degrees) + (minutes / 60.0) + (seconds / 3600.0);

            return sign * absDegrees;
        }


        public static List<Tuple<float, float>> calculateDSOpath(AstroWeather.DsoObject dso, DateTime dateUtc, List<DateTime> astrotimes, double lat, double lon)
        {
            List<Tuple<float, float>> trajectory = new List<Tuple<float, float>>();
            double observerLatitude = lat;
            double observerLongitude = lon;
            Observer observer = new Observer(observerLatitude, observerLongitude, 100);

            double raRad = ParseRA(raStr: dso.Ra!);
            double decRad = ParseDec(dso.Dec!);

            Astronomy.DefineStar(Body.Star1, raRad, decRad, 1000);

            DateTime sunset = astrotimes[0];
            DateTime sunrise = astrotimes[1];
            if ((sunrise - sunset).TotalDays < 0)
                sunrise = sunrise.AddDays(1);
            if ((sunrise - sunset).TotalDays > 1)
                sunrise = sunrise.AddDays(-1);
            TimeSpan step = TimeSpan.FromMinutes(20);
            for (DateTime currentTime = sunset; currentTime <= sunrise; currentTime += step)
            {
                AstroTime astroCurrent = new AstroTime(currentTime);
                Equatorial eq = Astronomy.Equator(Body.Star1, astroCurrent, observer, EquatorEpoch.OfDate, Aberration.Corrected);
                Topocentric topo = Astronomy.Horizon(astroCurrent, observer, eq.ra, eq.dec, Refraction.Normal);
                if (topo.altitude > 0)
                {
                    trajectory.Add(new Tuple<float, float>(
                        Convert.ToSingle(topo.azimuth),
                        Convert.ToSingle(topo.altitude)
                    ));
                }

            }
            return trajectory;
        }

        public static AstroWeather.DsoObject CalculateVisibilityAndAltitude(AstroWeather.DsoObject dso, DateTime dateUtc, List<DateTime> astrotimes, double lat, double lon)
        {
            double observerLatitude = lat;
            double observerLongitude = lon;
            Observer observer = new Observer(observerLatitude, observerLongitude, 100);

            double raRad = ParseRA(dso.Ra!);
            double decRad = ParseDec(dso.Dec!);


            Astronomy.DefineStar(Body.Star1, raRad, decRad, 1000);

            DateTime sunset = astrotimes[0];
            DateTime sunrise = astrotimes[1];
            if ((sunrise - sunset).TotalDays > 1)
                sunrise = sunrise.AddDays(-1);
            double visibleTime = 0;
            double maxAlt = double.MinValue;
            dso.SumAlitude = 0;

            TimeSpan step = TimeSpan.FromMinutes(5);
            for (DateTime currentTime = sunset; currentTime <= sunrise; currentTime += step)
            {
                AstroTime astroCurrent = new AstroTime(currentTime);
                Equatorial eq = Astronomy.Equator(Body.Star1, astroCurrent, observer, EquatorEpoch.OfDate, Aberration.Corrected);
                Topocentric topo = Astronomy.Horizon(astroCurrent, observer, eq.ra, eq.dec, Refraction.Normal);

                if (topo.altitude > maxAlt)
                    maxAlt = topo.altitude;

                if (topo.altitude > 20)
                {
                    dso.SumAlitude += topo.altitude;
                    visibleTime += step.TotalHours;
                }
            }


            double totalNight = (sunrise - sunset).TotalHours;
            dso.Visible = Math.Min((visibleTime / totalNight) * 100.0, 100.0);
            dso.MaxAlt = maxAlt;
            return dso;
        }

        public List<AstroWeather.DsoObject> GetTopVisibleObjects(DateTime dateUtc, List<DateTime> astrotimes, double lat, double lon)
        {
            var calculatedDsoObjects = _dsoObjects

                .Select(dso => CalculateVisibilityAndAltitude(dso, dateUtc, astrotimes, lat, lon))
                .ToList();
           
            return calculatedDsoObjects
         .Where(dso => !dso.photo)
         .OrderByDescending(dso => dso.SumAlitude)
         .ThenByDescending(dso => dso.Visible)
         .ToList();
        }
    }
}