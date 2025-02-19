using System.Diagnostics;
using System.Security.Cryptography;
using AstroWeather.Helpers;
using Microsoft.Maui.Storage;

namespace AstroWeather.Pages;

public partial class TargetsList : ContentPage
{
    double minsize = 0;
    double maxsize = 9999;
    private string _selectedDSO;
    public TargetsList()
    {
        InitializeComponent();
        
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InitDsoAsync();
    }
    private string PickerToFile()
    {
        if (_selectedDSO == "Herschel(400)") return "Herschel400.yaml";
        else if(_selectedDSO == "Messier") return "Messier.yaml";
        else return "GaryImm.yaml";

    }
    private async Task InitDsoAsync()
    {
        string fileName = PickerToFile();
        
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
        minsize = double.TryParse(MinSize.Text, out double min) ? min : 0.0;
        maxsize = double.TryParse(MaxSize.Text, out double max) ? max : 9999.0;
        await InitDsoAsync();
        await PopupView.FadeTo(0, 250, Easing.CubicInOut);
        PopupView.IsVisible = false;
    }
    void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            _selectedDSO = picker.Items[selectedIndex];
            Debug.WriteLine("_selectedDSO");
            Debug.WriteLine(_selectedDSO);
        }
        _ = InitDsoAsync();
    }
    private async void ClearPhotographed(object sender, EventArgs e)
    {
        string fileName = PickerToFile();
        await DsoCalculator.UpdateYamlFileAsync(fileName, dsoList =>
            {
                foreach (var dso in dsoList)
                {
                    dso.photo = false;
                }
            });
        
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