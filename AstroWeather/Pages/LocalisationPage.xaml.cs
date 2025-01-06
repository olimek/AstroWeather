using System.Collections.Generic;
using System.Text.Json;
using System.Xml.Linq;
using AstroWeather.Helpers;
using Microsoft.Maui.Controls;

namespace AstroWeather.Pages;

public partial class LocalisationPage : ContentPage
{
    public LocalisationPage()
    {
        InitializeComponent();
        LoadJsonData();
    }
    private void LoadJsonData()
    {
        // Example usage of LoadData to retrieve all localisation data
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

            //LogFileGetSet.StoreData(selectedItem.key, ));
            

            LocalisationListView.SelectedItem = null;
        }
    }
}
