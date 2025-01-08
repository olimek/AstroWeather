using System.Text.Json;
using AstroWeather.Helpers;


namespace AstroWeather.Pages;

public partial class LocalisationPage : ContentPage
{
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


    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is not null)
        {
            var selectedItem = (dynamic)e.SelectedItem;

            SelectedLabel.Text = $"Selected Localisation: {selectedItem.Key} ({selectedItem.Value})";


            LogFileGetSet.StoreData("DefaultLoc", new List<string>(new string[] { selectedItem.Key }));

            LocalisationListView.SelectedItem = null;
        }
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
