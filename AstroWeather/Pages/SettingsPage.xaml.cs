using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Windows.Input;
using AstroWeather.Helpers;

namespace AstroWeather.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public static ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
        private readonly LogFileGetSet _logFileGetSet = new LogFileGetSet();
        private SelectionChangedEventArgs? _selectedEventArgs;

        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadApiKeys();
            LoadJsonDataAsync();
            CheckDefaultLocAsync();
        }

        private async Task LoadApiKeys()
        {
            var apiKeys = await LogFileGetSet.LoadDataAsync("APIkey", () => new List<string>());
            string key = apiKeys != null && apiKeys.Count > 0 ? apiKeys[0] : string.Empty;
            APIkeyInput.Text = key;
        }

        private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            string myText = APIkeyInput.Text;
            await _logFileGetSet.StoreDataAsync("APIkey", new List<string> { myText });
            /*if (myText.Length == 25)
                await Navigation.PushModalAsync(new PopUp("Klucz API zapisany"));*/
        }

        private async void OnComputeClicked(object sender, EventArgs e)
        {
            // Przygotowanie popupu do dodania nowej lokalizacji
            nameInput.Text = string.Empty;
            LatitudeInput.Text = string.Empty;
            LongitudeInput.Text = string.Empty;
            Info.Text = string.Empty;
            DeleteButton.IsVisible = false; // Nowy wpis – brak opcji usuwania
            PopupView.IsVisible = true;
            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
        }

        private async void OnClosePopupClicked(object sender, EventArgs e)
        {
            string name = nameInput.Text;
            string lat = LatitudeInput.Text.Replace(".", ",");
            string lon = LongitudeInput.Text.Replace(".", ",");
            string info = Info.Text;

            if (!string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(info))
                {
                    await _logFileGetSet.StoreDataAsync($"Info_{name}", new List<string> { info });
                }
                await _logFileGetSet.StoreDataAsync($"Localisation_{name}", new List<string> { lat, lon });

                // Ustawienie jako domyślnej, jeśli brak domyślnej lokalizacji
                var defaultLoc = await LogFileGetSet.LoadDataAsync("DefaultLoc", () => new List<string>());
                if (defaultLoc == null || defaultLoc.Count == 0)
                {
                    await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { name });
                    await DisplayAlert("Info", $"Lokalizacja '{name}' została ustawiona jako domyślna.", "OK");
                }
                else if (_selectedEventArgs != null)
                {
                    // Jeśli edytujemy istniejącą lokalizację, zaktualizuj domyślną
                    await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { name });
                }
            }
            await PopupView.FadeTo(0, 250, Easing.CubicInOut);
            PopupView.IsVisible = false;
            await LoadJsonDataAsync();
            await CheckDefaultLocAsync();
        }

        private async void OnGetLocationClicked(object sender, EventArgs e)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                try
                {
                    var location = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10))
                    );

                    if (location != null)
                    {
                        double latitude = location.Latitude;
                        double longitude = location.Longitude;
                        await DisplayAlert("Lokalizacja pobrana",
                            $"Szerokość: {latitude}, Długość: {longitude}",
                            "OK");

                        bool shouldSave = await DisplayAlert(
                            "Zapisz lokalizację?",
                            "Czy chcesz zapisać tę lokalizację w aplikacji?",
                            "Tak",
                            "Nie");

                        if (shouldSave)
                        {
                            LatitudeInput.Text = latitude.ToString(CultureInfo.InvariantCulture);
                            LongitudeInput.Text = longitude.ToString(CultureInfo.InvariantCulture);
                            nameInput.Text = $"GPS_{DateTime.Now:yyyyMMdd_HHmm}";
                            DeleteButton.IsVisible = false; // Nowy wpis z GPS
                            PopupView.IsVisible = true;
                            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Błąd", "Nie udało się pobrać lokalizacji.", "OK");
                    }
                }
                catch (FeatureNotSupportedException)
                {
                    await DisplayAlert("Błąd", "Urządzenie nie obsługuje GPS.", "OK");
                }
                catch (FeatureNotEnabledException)
                {
                    await DisplayAlert("GPS wyłączony", "Proszę włączyć GPS w ustawieniach urządzenia.", "OK");
                }
                catch (PermissionException)
                {
                    await DisplayAlert("Brak uprawnień", "Aplikacja nie ma uprawnień do pobierania lokalizacji.", "OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Błąd pobierania lokalizacji: {ex.Message}");
                    await DisplayAlert("Błąd", "Wystąpił nieoczekiwany błąd przy pobieraniu lokalizacji.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Brak uprawnień", "Nie przyznano uprawnień do lokalizacji.", "OK");
            }
        }

        private async Task LoadJsonDataAsync()
        {
            var jsonData = await LogFileGetSet.LoadAllDataAsync();

            var filteredData = jsonData
                 .Where(kvp => kvp.Key.Contains("Localisation_"))
                 .ToDictionary(kvp => kvp.Key.Replace("Localisation_", ""), static kvp =>
                 {
                     if (kvp.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                     {
                         return jsonElement.EnumerateArray()
                             .Select(v =>
                             {
                                 if (v.ValueKind == JsonValueKind.Number)
                                 {
                                     return v.GetDouble();
                                 }
                                 else if (v.ValueKind == JsonValueKind.String && double.TryParse(v.GetString(), out double result))
                                 {
                                     return result;
                                 }
                                 return (double?)null;
                             })
                             .Where(v => v.HasValue)
                             .Select(static v => v!.Value)
                             .ToList();
                     }
                     return new List<double>();
                 });

            var items = filteredData.Select(kvp => new
            {
                Key = kvp.Key,
                Value = string.Join(", ", kvp.Value.Select(v => v.ToString().Replace(",", ".")))
            }).ToList();

            LocalisationCollectionView.ItemsSource = items;
        }

        private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not null)
            {
                var selectedItem = e.CurrentSelection.FirstOrDefault() as dynamic;
                SelectedLabel.Text = $"Selected Localisation: {selectedItem!.Key} ({selectedItem!.Value})";
                _selectedEventArgs = e;
                string[] parts = selectedItem.Value.Split(',');
                nameInput.Text = selectedItem.Key;
                LatitudeInput.Text = parts[0].Trim();
                LongitudeInput.Text = parts[1].Trim();
                // Jeśli istnieją dodatkowe informacje, ładujemy je
                var infoData = await LogFileGetSet.LoadDataAsync($"Info_{selectedItem.Key}", () => new List<string>());
                Info.Text = (infoData != null && infoData.Count > 0) ? infoData[0] : string.Empty;
                DeleteButton.IsVisible = true; // Widoczny przy edycji istniejącej lokalizacji
                PopupView.IsVisible = true;
                await PopupView.FadeTo(1, 250, Easing.CubicInOut);
            }
            LocalisationCollectionView.SelectedItem = null;
        }

        private async void DeleteEntity(object sender, EventArgs e)
        {
            if (_selectedEventArgs?.CurrentSelection.FirstOrDefault() is not null)
            {
                var selectedItem = _selectedEventArgs.CurrentSelection.FirstOrDefault() as dynamic;
                string key = $"Localisation_{selectedItem!.Key}";
                var defaultLoc = await LogFileGetSet.LoadDataAsync("DefaultLoc", () => new List<string>());
                bool isDefault = defaultLoc != null && defaultLoc.Count > 0 && defaultLoc[0] == selectedItem.Key;

                await _logFileGetSet.RemoveDataAsync<List<string>>(key);
                await _logFileGetSet.RemoveDataAsync<List<string>>($"Info_{selectedItem.Key}");

                await PopupView.FadeTo(1, 250, Easing.CubicInOut);
                PopupView.IsVisible = false;
                _selectedEventArgs = null;
                await LoadJsonDataAsync();
                if (isDefault)
                {
                    var allData = await LogFileGetSet.LoadAllDataAsync();
                    var localisationsLeft = allData
                        .Where(kvp => kvp.Key.StartsWith("Localisation_"))
                        .Select(kvp => kvp.Key.Replace("Localisation_", ""))
                        .ToList();

                    if (localisationsLeft.Any())
                    {
                        string newDefaultName = localisationsLeft.First();
                        await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { newDefaultName });
                        await DisplayAlert("Info", $"Lokalizacja '{newDefaultName}' została nową lokalizacją domyślną.", "OK");
                    }
                    else
                    {
                        bool shouldAdd = await DisplayAlert(
                            "Brak lokalizacji",
                            "Usunięto ostatnią lokalizację. Czy chcesz dodać nową?",
                            "Tak",
                            "Nie");

                        if (shouldAdd)
                        {
                            nameInput.Text = "";
                            LatitudeInput.Text = "";
                            LongitudeInput.Text = "";
                            Info.Text = "";

                            PopupView.IsVisible = true;
                            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
                        }
                    }
                }
                await LoadJsonDataAsync();
                await CheckDefaultLocAsync();
            }
        }

        private async Task CheckDefaultLocAsync()
        {
            var defaultLatLon = await LogFileGetSet.LoadDefaultLocAsync();
            var defaultLatLonName = await LogFileGetSet.LoadDefaultLocNameAsync();
            if (defaultLatLon != null && defaultLatLon.Count > 1)
            {
                SelectedLabel.Text = $"Selected Localisation: {defaultLatLonName} ({defaultLatLon[0]}, {defaultLatLon[1]})";
            }
        }
    }
}
