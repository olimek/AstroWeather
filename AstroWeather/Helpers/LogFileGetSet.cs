using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstroWeather.Helpers
{
    class LogFileGetSet
    {
        public static void StoreData<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.");
                if (value == null) throw new ArgumentNullException(nameof(value));

                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.Name);
                string filePath = Path.Combine(directoryPath, "data.json");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                Dictionary<string, T> data = ReadData<T>(filePath);
                data[key] = value;

                WriteData(filePath, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing data: {ex.Message}");
            }
        }

        private static Dictionary<string, T> ReadData<T>(string filePath)
        {
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                string existingJson = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Dictionary<string, T>>(existingJson) ?? new Dictionary<string, T>();
            }

            return new Dictionary<string, T>();
        }

        private static void WriteData<T>(string filePath, Dictionary<string, T> data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }



        // Load data based on a key as an identifier
        public static T LoadData<T>(string key, T defaultValue = default)
        {
            try
            {
                // Define the file path
                string filePath = Path.Combine(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.Name),
                    "data.json");

                // Check if the file exists
                if (File.Exists(filePath))
                {
                    // Read the JSON file and deserialize it into a dictionary
                    string json = File.ReadAllText(filePath);
                    var rootObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    // Check if the key exists
                    if (rootObject != null && rootObject.TryGetValue(key, out JsonElement valueElement))
                    {
                        // Handle value types
                        if (valueElement.ValueKind == JsonValueKind.Array)
                        {
                            // For arrays, deserialize to the target type
                            return valueElement.Deserialize<T>();
                        }
                        else if (valueElement.ValueKind == JsonValueKind.String)
                        {
                            // For strings, convert directly to the target type
                            return JsonSerializer.Deserialize<T>($"\"{valueElement.GetString()}\"");
                        }
                        else
                        {
                            // For other types (e.g., objects), try deserialization
                            return valueElement.Deserialize<T>();
                        }
                    }
                }

                return defaultValue; // Return default value if key not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load data: {ex.Message}");
                return defaultValue;
            }
        }

        public static Dictionary<string, object> LoadAllData()
        {
            try
            {
                string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.Name), "data.json");
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string json = reader.ReadToEnd();
                        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                        return data ?? new Dictionary<string, object>();
                    }
                }

                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load all data: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }
        /*
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            StoreData<List<string>>("hello", new List<string>() { "ASD", "AAA" });
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            var value = LoadData<List<string>>("hello", new List<string>());
        }

}*/
    }
}
