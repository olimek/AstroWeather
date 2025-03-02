using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
#if ANDROID
using Android.Gms.Common.Util;
#endif
using AstroWeather.Helpers;


namespace AstroWeather.Pages
{
    public class LocalisationItem
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
    public partial class LocalisationPage : ContentPage
    {
        private SelectionChangedEventArgs? _selectedEventArgs;
        private readonly LogFileGetSet _logFileGetSet = new LogFileGetSet();

        public LocalisationPage()
        {

            InitializeComponent();
            BindingContext = this;
            _ = CheckDefaultLocAsync();

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadJsonDataAsync();
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

                        // Wyświetlamy komunikat z aktualną lokalizacją
                        await DisplayAlert("Lokalizacja pobrana",
                            $"Szerokość: {latitude}, Długość: {longitude}",
                            "OK");

                        // Pytamy, czy zapisać
                        bool shouldSave = await DisplayAlert(
                            "Zapisz lokalizację?",
                            "Czy chcesz zapisać tę lokalizację w aplikacji?",
                            "Tak",
                            "Nie");

                        if (shouldSave)
                        {
                            // Podstawiamy domyślnie odczytane współrzędne do pól
                            LatitudeInput.Text = latitude.ToString(CultureInfo.InvariantCulture);
                            LongitudeInput.Text = longitude.ToString(CultureInfo.InvariantCulture);

                            // Możesz ustawić również domyślną nazwę, np. "GPS_20250301_1200"
                            nameInput.Text = $"GPS_{DateTime.Now:yyyyMMdd_HHmm}";

                            // Otwieramy istniejący popup
                            PopupView.IsVisible = true;
                            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
                        }
                    }
                    else
                    {
                        // location == null
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

            foreach (var kvp in filteredData)
            {
                Debug.WriteLine($"Key: {kvp.Key}, Value: {string.Join(", ", kvp.Value)}");
            }

            LocalisationCollectionView.ItemsSource = filteredData.Select(kvp => new
            {
                kvp.Key,
                Value = string.Join(", ", kvp.Value.Select(v => v.ToString().Replace(",", ".")))
            }).ToList();
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
                await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { selectedItem.Key });
            }

            LocalisationCollectionView.SelectedItem = null;
            PopupView.IsVisible = true;
            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
        }

        private async void OnComputeClicked(object sender, EventArgs e)
        {
            PopupView.IsVisible = true;
            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
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
                LocalisationCollectionView.SelectedItem = null;
                _ = LoadJsonDataAsync();
                if (isDefault)
                {
                    // Czy są jeszcze jakieś lokalizacje w bazie?
                    var allData = await LogFileGetSet.LoadAllDataAsync();
                    var localisationsLeft = allData
                        .Where(kvp => kvp.Key.StartsWith("Localisation_"))
                        .Select(kvp => kvp.Key.Replace("Localisation_", ""))
                        .ToList();

                    if (localisationsLeft.Any())
                    {
                        // Ustaw pierwszą z pozostałych jako domyślną
                        string newDefaultName = localisationsLeft.First();
                        await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { newDefaultName });
                        await DisplayAlert("Info", $"Lokalizacja '{newDefaultName}' została nową lokalizacją domyślną.", "OK");
                    }
                    else
                    {
                        // Nie ma żadnej lokalizacji – pytamy, czy dodać nową
                        bool shouldAdd = await DisplayAlert(
                            "Brak lokalizacji",
                            "Usunięto ostatnią lokalizację. Czy chcesz dodać nową?",
                            "Tak",
                            "Nie");

                        if (shouldAdd)
                        {
                            // Otwieramy popup z pustymi polami
                            nameInput.Text = "";
                            LatitudeInput.Text = "";
                            LongitudeInput.Text = "";
                            Info.Text = "";

                            PopupView.IsVisible = true;
                            await PopupView.FadeTo(1, 250, Easing.CubicInOut);
                        }
                    }
                }
                _ = LoadJsonDataAsync();
                _ = CheckDefaultLocAsync();
                // Odśwież widok / cofnięcie
                await Shell.Current.GoToAsync("//LocalisationPage");
            }
        }

        private async void OnClosePopupClicked(object sender, EventArgs e)
        {
            string name = nameInput.Text;
            string lat = LatitudeInput.Text.Replace(".", ",");
            string lon = LongitudeInput.Text.Replace(".", ",");

            if (!string.IsNullOrEmpty(Info.Text))
            {
                string info = Info.Text.Replace(".", ",");
                await _logFileGetSet.StoreDataAsync($"Info_{name}", new List<string> { info });
            }

            await _logFileGetSet.StoreDataAsync($"Localisation_{name}", new List<string> { lat, lon });
            var defaultLatLon = await LogFileGetSet.LoadDefaultLocAsync();
            if (defaultLatLon == null || defaultLatLon.Count == 0)
            {
                // Jeśli nie było, ustaw nowo zapisaną lokalizację jako domyślną
                await _logFileGetSet.StoreDataAsync("DefaultLoc", new List<string> { name });
                await DisplayAlert("Info", $"Lokalizacja '{name}' została ustawiona jako domyślna.", "OK");
            }
            await PopupView.FadeTo(0, 250, Easing.CubicInOut);
            LocalisationCollectionView.SelectedItem = null;
            _ = LoadJsonDataAsync();
            _ = CheckDefaultLocAsync();
            await Shell.Current.GoToAsync("//LocalisationPage");

            PopupView.IsVisible = false;
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