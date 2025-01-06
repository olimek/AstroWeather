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
        string test = LogFileGetSet.LoadData<string>("APIkey");
        Console.WriteLine(test);
        BindingContext = this;
    }

    void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        
        string myText = APIkeyInput.Text;
        LogFileGetSet.StoreData("APIkey", new List<string>(new string[] { myText }));
    }
    void OnEntryCompleted(object sender, EventArgs e)
    {
        
    }
    private async void OnComputeClicked(object sender, EventArgs e)
    {
        PopupView.IsVisible = true;

        // Animacja fade-in
        await PopupView.FadeTo(1, 250, Easing.CubicInOut);
    }
    private async void OnClosePopupClicked(object sender, EventArgs e)
    {
        string name = nameInput.Text;

        string Lat = LatitudeInput.Text.Replace(".", ",");

        string Lon = LongitudeInput.Text.Replace(".", ",");

        LogFileGetSet.StoreData($"Localisation_{name}", new List<string>(new string[] { Lat, Lon, "0" }));

        // Animacja fade-out
        await PopupView.FadeTo(0, 250, Easing.CubicInOut);

        // Ukryj okienko
        PopupView.IsVisible = false;
    }
}