using System.Diagnostics;
using AstroWeather.Helpers;

namespace AstroWeather.Pages;

public partial class TargetsList : ContentPage
{
    public TargetsList()
    {
        InitializeComponent();
        Initdso();
    }

    public void Initdso()
    {
        string filePath = "Helpers\\GaryImm.yaml";
        // Utwórz instancję klasy DsoCalculator
        DsoCalculator dsoCalculator = new DsoCalculator(filePath);

        // Pobierz aktualny czas UTC
        DateTime now = DateTime.UtcNow;

        // Zakładam, że WeatherRouter.GetAstroTimes jest metodą zwracającą czasy astronomiczne dla zachodu i wschodu słońca
        var astroTimes = WeatherRouter.GetAstroTimes(now, true);
        var LAT = WeatherRouter.lat;
        var LON = WeatherRouter.lon;
        // Uzyskaj listę najlepiej widocznych obiektów DSO
        List<DsoObject> calculatedDSO = dsoCalculator.GetTopVisibleObjects(now, astroTimes, 20, LAT, LON);

        // Wyświetl wyniki
        foreach (var dso in calculatedDSO)
        {
            Debug.WriteLine($"Name: {dso.Name}, Visible: {dso.Visible}%, Max Altitude: {dso.MaxAlt}°. Magnitude: {dso.Mag}");
        }
        DsoCollectionView.ItemsSource = calculatedDSO;
    }
}