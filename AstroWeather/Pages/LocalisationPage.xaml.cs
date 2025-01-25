using System.Text.Json;
using AstroWeather.Helpers;


namespace AstroWeather.Pages;

public partial class LocalisationPage : ContentPage
{
    private SelectedItemChangedEventArgs _selectedEventArgs;
    public LocalisationPage()
    {
        InitializeComponent();
        LoadJsonData();
        CheckDefaultLoc();
    }
    private void LoadJsonData()
    {

        var jsonData = LogFileGetSet.LoadAllData();

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

        LocalisationListView.ItemsSource = filteredData.Select(kvp => new
        {
            kvp.Key,
            Value = string.Join(", ", kvp.Value.Select(v => v.ToString().Replace(",", ".")))
        }).ToList();
    }


    private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {

        
        if (e.SelectedItem is not null)
        {
            var selectedItem = (dynamic)e.SelectedItem;

            SelectedLabel.Text = $"Selected Localisation: {selectedItem.Key} ({selectedItem.Value})";
            _selectedEventArgs = e;
            string[] parts = selectedItem.Value.Split(',');
            nameInput.Text = selectedItem.Key;
            LatitudeInput.Text = parts[0].Trim();
            LongitudeInput.Text = parts[1].Trim();
        }

        PopupView.IsVisible = true;
        await PopupView.FadeTo(1, 250, Easing.CubicInOut);
        /*
        if (e.SelectedItem is not null)
        {
            var selectedItem = (dynamic)e.SelectedItem;

            SelectedLabel.Text = $"Selected Localisation: {selectedItem.Key} ({selectedItem.Value})";


            LogFileGetSet.StoreData("DefaultLoc", new List<string>(new string[] { selectedItem.Key }));

            LocalisationListView.SelectedItem = null;
        }*/
    }

    private async void OnComputeClicked(object sender, EventArgs e)
    {
        PopupView.IsVisible = true;
        
        await PopupView.FadeTo(1, 250, Easing.CubicInOut);
    }

    private async void DeleteEntity(object sender, EventArgs e)
    {
        var key = ((dynamic)_selectedEventArgs.SelectedItem);
        PopupView.IsVisible = false;


        await PopupView.FadeTo(1, 250, Easing.CubicInOut);
    }
    private async void OnClosePopupClicked(object sender, EventArgs e)
    {
        string name = nameInput.Text;

        string Lat = LatitudeInput.Text.Replace(".", ",");

        string Lon = LongitudeInput.Text.Replace(".", ",");

        LogFileGetSet.StoreData($"Localisation_{name}", new List<string>(new string[] { Lat, Lon }));


        await PopupView.FadeTo(0, 250, Easing.CubicInOut);


        PopupView.IsVisible = false;
    }

    private void CheckDefaultLoc()
    {
        var DefaultLatLon = LogFileGetSet.LoadDefaultLoc();

        if (DefaultLatLon != null)
        {
            SelectedLabel.Text = $"Default Localisation: ({DefaultLatLon[0]}, {DefaultLatLon[1]})";
        }
    }

}
