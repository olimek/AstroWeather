using System.Text.Json;
using AstroWeather.Helpers;

namespace AstroWeather.Pages
{
    public partial class LocalisationPage : ContentPage
    {
        private SelectionChangedEventArgs _selectedEventArgs;
        private readonly LogFileGetSet _logFileGetSet = new LogFileGetSet();

        public LocalisationPage()
        {

            InitializeComponent();
            BindingContext = this;
            CheckDefaultLocAsync();

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadJsonDataAsync();
        }
        private async Task LoadJsonDataAsync()
        {

            var jsonData = await LogFileGetSet.LoadAllDataAsync();

            var filteredData = jsonData
                 .Where(kvp => kvp.Key.Contains("Localisation_"))
                 .ToDictionary(kvp => kvp.Key.Replace("Localisation_", ""), kvp =>
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
                             .Select(v => v.Value)
                             .ToList();
                     }
                     return new List<double>();
                 });

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

                SelectedLabel.Text = $"Selected Localisation: {selectedItem.Key} ({selectedItem.Value})";
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
                string key = $"Localisation_{selectedItem.Key}";

                await _logFileGetSet.RemoveDataAsync<List<string>>(key);
                await _logFileGetSet.RemoveDataAsync<List<string>>($"Info_{selectedItem.Key}");

                await PopupView.FadeTo(1, 250, Easing.CubicInOut);
                PopupView.IsVisible = false;
                LocalisationCollectionView.SelectedItem = null;
                LoadJsonDataAsync();
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

            await PopupView.FadeTo(0, 250, Easing.CubicInOut);
            LocalisationCollectionView.SelectedItem = null;
            LoadJsonDataAsync();
            await Shell.Current.GoToAsync("//LocalisationPage");

            PopupView.IsVisible = false;
        }

        private async void CheckDefaultLocAsync()
        {
            var defaultLatLon = await LogFileGetSet.LoadDefaultLocAsync();

            if (defaultLatLon != null && defaultLatLon.Count() > 1)
            {
                SelectedLabel.Text = $"Default Localisation: ({defaultLatLon[0]}, {defaultLatLon[1]})";
            }
        }
    }
}