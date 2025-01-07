

using AstroWeather.Helpers;
using CosineKitty;

namespace AstroWeather
{

    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

            var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();

            // Sprawdzenie poprawności danych lokalizacji
            if (DefaultLatLon == null || DefaultLatLon.Count < 2)
            {
                throw new Exception("DefaultLatLon is null or incomplete.");
            }

            double latitude = Convert.ToDouble(DefaultLatLon[0]);
            double longitude = Convert.ToDouble(DefaultLatLon[1]);

            
            DateTime startDate = DateTime.Now;
            startDate = startDate.AddDays(-1);

            // Iteracja przez kilka dni
            double elevation = 0; // Elevation above sea level in meters

            Observer observer = new Observer(latitude, longitude, elevation);
            DateTime localMoonset = DateTime.MinValue;
            DateTime localMoonrise = DateTime.MinValue; 


            // Loop through multiple days
            
                AstroTime startTime = new AstroTime(startDate);
                
                // Search for Moonrise
                var moonrise = Astronomy.SearchRiseSet(
                    Body.Moon,
                    observer,
                    Direction.Rise,
                    startTime,
                    1.0, // Limit search to 1 day
                    0.0  // Altitude for horizon
                );
                if (moonrise != null)
                {
                    DateTime utcMoonrise = moonrise.ToUtcDateTime();
                    localMoonrise = TimeZoneInfo.ConvertTimeFromUtc(utcMoonrise, localTimeZone);
                }
                // Search for Moonset
                var moonset = Astronomy.SearchRiseSet(
                    Body.Moon,
                    observer,
                    Direction.Set,
                    startTime,
                    1.0, // Limit search to 1 day
                    0.0  // Altitude for horizon
                );
                if (moonset != null)
                {
                    DateTime utcMoonset = moonset.ToUtcDateTime();
                    localMoonset = TimeZoneInfo.ConvertTimeFromUtc(utcMoonset, localTimeZone);
                }
                moonrisel.Text = localMoonrise.ToString();
                moonsetl.Text = localMoonset.ToString();
            

        }
    }
}
