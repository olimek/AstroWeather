using AstroWeather.Helpers;
using CosineKitty;


namespace AstroWeather

{
    public partial class App : Application
    {
        public App()
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

            var results = new List<object>(); // Zmienna przechowująca wyniki
            DateTime startDate = new DateTime(2025, 01, 01);

            // Iteracja przez kilka dni
            double elevation = 0; // Elevation above sea level in meters

            Observer observer = new Observer(latitude, longitude, elevation);

            

            // Loop through multiple days
            for (int i = 0; i < 9; i++)
            {
                DateTime currentDate = startDate.AddDays(i);
                AstroTime startTime = new AstroTime(currentDate);
                DateTime localMoonset, localMoonrise;
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
                

            }
        }
    
    



        

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}