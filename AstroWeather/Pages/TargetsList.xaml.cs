using System.Diagnostics;
using System.Security.Cryptography;
using AstroWeather.Helpers;
using Microsoft.Maui.Storage;

namespace AstroWeather.Pages;

public partial class TargetsList : ContentPage
{
    double minsize = 0;
    double maxsize = 9999;
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
        string fileName = "Herschel400.yaml";
        //string fileName = "GaryImm.yaml";
        DsoCalculator dsoCalculator = await DsoCalculator.CreateAsync(fileName);
        DateTime now = DateTime.UtcNow;
        var astroTimes = WeatherRouter.GetAstroTimes(now, true);
        var lat = WeatherRouter.Lat;
        var lon = WeatherRouter.Lon;

        List<DsoObject> calculatedDSO = dsoCalculator.GetTopVisibleObjects(now, astroTimes, lat, lon);

        DsoCollectionView.ItemsSource = calculatedDSO.Where(dso => dso.Size >= minsize && dso.Size <= maxsize);
    }
    private async void OnComputeClicked2(object sender, EventArgs e)
    {
        PopupView.IsVisible = true;
        await PopupView.FadeTo(1, 250, Easing.CubicInOut);
    }

    private async void OnClosePopupClicked(object sender, EventArgs e)
    {
        minsize = Convert.ToDouble( MinSize.Text);
        maxsize = Convert.ToDouble(MaxSize.Text);
        await InitDsoAsync();
        await PopupView.FadeTo(0, 250, Easing.CubicInOut);
        PopupView.IsVisible = false;
    }

    private async void ClearPhotographed(object sender, EventArgs e)
    {
        
            await DsoCalculator.UpdateYamlFileAsync("Herschel400.yaml", dsoList =>
            {
                foreach (var dso in dsoList)
                {
                    dso.photo = false;
                }
            });
        /*await DsoCalculator.UpdateYamlFileAsync("GaryImm.yaml", dsoList =>
        {
            foreach (var dso in dsoList)
            {
                dso.photo = false;
            }
        });*/
        await InitDsoAsync();
        await PopupView.FadeTo(0, 250, Easing.CubicInOut);
        PopupView.IsVisible = false;
    }
    private async void DSOListView_ItemTapped(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is DsoObject selectedItem)
        {
            _ = new Dictionary<string, object>
                {
                    { "selectedDSO", selectedItem }
                };

            await Navigation.PushAsync(new DsoChartPage(selectedItem));
        }

            ((CollectionView)sender).SelectedItem = null;
    }

}