using System.Diagnostics;
using AstroWeather.Helpers;

namespace AstroWeather.Pages;

public partial class TargetsList : ContentPage
{
    public TargetsList()
    {
        InitializeComponent();
        InitDsoAsync();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InitDsoAsync();
    }

    private async Task InitDsoAsync()
    {
        // Używamy tylko nazwy pliku, gdyż plik jest dodany jako zasób aplikacji.
        string fileName = "GaryImm.yaml";

        // Utwórz instancję klasy DsoCalculator asynchronicznie
        DsoCalculator dsoCalculator = await DsoCalculator.CreateAsync(fileName);

        // Pobierz aktualny czas UTC
        DateTime now = DateTime.UtcNow;

        // Zakładam, że WeatherRouter.GetAstroTimes jest metodą zwracającą czasy astronomiczne (np. zachodu i wschodu słońca)
        var astroTimes = WeatherRouter.GetAstroTimes(now, true);
        var lat = WeatherRouter.lat;
        var lon = WeatherRouter.lon;

        // Uzyskaj listę najlepiej widocznych obiektów DSO
        List<DsoObject> calculatedDSO = dsoCalculator.GetTopVisibleObjects(now, astroTimes, 20, lat, lon);

        // Wyświetl wyniki w debugowaniu
        foreach (var dso in calculatedDSO)
        {
            Debug.WriteLine($"Name: {dso.Name}, Visible: {dso.Visible}%, Max Altitude: {dso.MaxAlt}°, Magnitude: {dso.Mag}");
        }

        // Ustaw źródło danych np. dla CollectionView
        DsoCollectionView.ItemsSource = calculatedDSO;
    }

}