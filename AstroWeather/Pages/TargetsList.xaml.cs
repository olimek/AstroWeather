using System.Diagnostics;
using AstroWeather.Helpers;

namespace AstroWeather.Pages;

public partial class TargetsList : ContentPage
{
    public TargetsList()
    {
        InitializeComponent();
        
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InitDsoAsync();
    }

    private async Task InitDsoAsync()
    {
        string fileName = "GaryImm.yaml";
        DsoCalculator dsoCalculator = await DsoCalculator.CreateAsync(fileName);
        DateTime now = DateTime.UtcNow;
        var astroTimes = WeatherRouter.GetAstroTimes(now, true);
        var lat = WeatherRouter.lat;
        var lon = WeatherRouter.lon;

        List<DsoObject> calculatedDSO = dsoCalculator.GetTopVisibleObjects(now, astroTimes, 20, lat, lon);

        DsoCollectionView.ItemsSource = calculatedDSO;
    }

    private async void DSOListView_ItemTapped(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is DsoObject selectedItem)
        {
            var parameters = new Dictionary<string, object>
                {
                    { "selectedDSO", selectedItem }
                };

            await Navigation.PushAsync(new DsoChartPage(selectedItem));
        }

            ((CollectionView)sender).SelectedItem = null;
    }

}