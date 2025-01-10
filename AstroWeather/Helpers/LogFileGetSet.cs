using System.Text.Json;

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

        public static T LoadData<T>(string key, T defaultValue = default)
        {
            try
            {

                string filePath = Path.Combine(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.Name),
                    "data.json");

                if (File.Exists(filePath))
                {

                    string json = File.ReadAllText(filePath);
                    var rootObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (rootObject != null && rootObject.TryGetValue(key, out JsonElement valueElement))
                    {

                        if (valueElement.ValueKind == JsonValueKind.Array)
                        {
                            return valueElement.Deserialize<T>();
                        }
                        else if (valueElement.ValueKind == JsonValueKind.String)
                        {
                            return JsonSerializer.Deserialize<T>($"\"{valueElement.GetString()}\"");
                        }
                        else
                        {
                            return valueElement.Deserialize<T>();
                        }
                    }
                }

                return defaultValue;
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

        public static List<string>? LoadDefaultLoc()
        {
            var defaultLoc = LogFileGetSet.LoadData("DefaultLoc", new List<string>());
            if (defaultLoc != null && defaultLoc.Count > 0)
            {
                var locloc = LogFileGetSet.LoadData($"Localisation_{defaultLoc[0]}", new List<string>());
                return locloc;

            }

            return null;
        }

        public static string getAPIkey(string which)
        {
            string? APIkey = null;
            if (which == "astro") {
                APIkey = LogFileGetSet.LoadData("MOONAPIkey", new List<string>())[0];
            }
            else if (which == "weather") {
                APIkey = LogFileGetSet.LoadData("APIkey", new List<string>())[0];
            }

            return APIkey;
            
        }

    }
}
