using System.Collections.Generic;
using System.Windows.Input;
using AstroWeather.Helpers;
using Microsoft.Maui.Controls;

namespace AstroWeather.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public static ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
        private readonly LogFileGetSet _logFileGetSet = new LogFileGetSet(); 

        public SettingsPage()
        {
            InitializeComponent();
            _ = LoadApiKeys();
            BindingContext = this;
            

        }

        private async Task LoadApiKeys()
        {
            var apiKeys = await LogFileGetSet.LoadDataAsync("APIkey", () => new List<string>());
            string test = apiKeys != null && apiKeys.Count > 0 ? apiKeys[0] : string.Empty;
            APIkeyInput.Text = test;
         
        }

        private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            string myText = APIkeyInput.Text;
            await _logFileGetSet.StoreDataAsync("APIkey", new List<string> { myText });
            await Navigation.PushModalAsync(new AstroWeather.Pages.PopUp("Klucz API zapisany"));
        }

        private async void OnComputeClicked(object sender, EventArgs e)
        {
            PopupView.IsVisible = true;
            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
        }

        private async void OnClosePopupClicked(object sender, EventArgs e)
        {
            string name = nameInput.Text;
            string lat = LatitudeInput.Text.Replace(".", ",");
            string lon = LongitudeInput.Text.Replace(".", ",");

            if (!string.IsNullOrEmpty(name))
            {
                await _logFileGetSet.StoreDataAsync($"Localisation_{name}", new List<string> { lat, lon });
                await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { name });
            }

            await PopupView.FadeTo(0, 250, Easing.CubicInOut);
            PopupView.IsVisible = false;
            await Navigation.PushModalAsync(new AstroWeather.Pages.PopUp("Dodano nową lokalizację"));
        }
    }
}