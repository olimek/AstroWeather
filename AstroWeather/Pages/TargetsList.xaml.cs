using AstroWeather.Helpers;

namespace AstroWeather.Pages;

public partial class TargetsList : ContentPage
{
	public TargetsList()
	{
		InitializeComponent();
        Initdso();
    }

    public void Initdso() {
        string filePath = "Helpers\\GaryImm.yaml";

        // Wczytaj dane z pliku YAML
        var dsoList = DsoYamlLoader.LoadDsoData(filePath); 

        // Wyszukaj obiekt po nazwie (przykład: "NGC 40")
        string objectName = "NGC 40";
        var dso = DsoYamlLoader.GetDsoByName(objectName, dsoList);

    }
}