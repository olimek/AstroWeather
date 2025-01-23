using System.Windows.Input;
using AstroWeather.Helpers;
using Microsoft.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;
namespace AstroWeather.Pages;

public partial class SettingsPage : ContentPage
{
    public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
    public SettingsPage()
    {
        InitializeComponent();
        var apiKeys = LogFileGetSet.LoadData<List<string>>("APIkey", () => new List<string>());

        // Sprawdzenie, czy lista nie jest pusta, i pobranie pierwszego klucza
        string test = apiKeys != null && apiKeys.Count > 0 ? apiKeys[0] : string.Empty;

        BindingContext = this;
    }

    void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        
        string myText = APIkeyInput.Text;
        LogFileGetSet.StoreData("APIkey", new List<string>(new string[] { myText }));
    }

    void OnEntryTextChangedMOON(object sender, TextChangedEventArgs e)
    {

        string myText = APIMOONkeyInput.Text;
        LogFileGetSet.StoreData("MOONAPIkey", new List<string>(new string[] { myText }));
    }

    private async void OnComputeClicked(object sender, EventArgs e)
    {
        PopupView.IsVisible = true;

        
        await PopupView.FadeTo(1, 250, Easing.CubicInOut);
    }
    private async void OnClosePopupClicked(object sender, EventArgs e)
    {
        string name = nameInput.Text;

        string Lat = LatitudeInput.Text.Replace(".", ",");

        string Lon = LongitudeInput.Text.Replace(".", ",");

        LogFileGetSet.StoreData($"Localisation_{name}", new List<string>(new string[] { Lat, Lon}));

        
        await PopupView.FadeTo(0, 250, Easing.CubicInOut);

        
        PopupView.IsVisible = false;
    }
}