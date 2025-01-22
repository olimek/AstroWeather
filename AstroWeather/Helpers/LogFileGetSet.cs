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

        public static T LoadData<T>(string key, Func<T> defaultFactory)
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

                return defaultFactory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load data: {ex.Message}");
                return defaultFactory();
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

        public static List<double>? LoadDefaultLoc()
        {
            // Ładujemy domyślną lokalizację (może być `null` lub pusta lista)
            var defaultLoc = LogFileGetSet.LoadData<List<string>>("DefaultLoc", () => new List<string>());

            // Sprawdzamy, czy istnieje przynajmniej jeden element
            if (defaultLoc != null && defaultLoc.Count > 0)
            {
                // Ładujemy lokalizację na podstawie pierwszego elementu z defaultLoc
                var rawData = LogFileGetSet.LoadData($"Localisation_{defaultLoc[0]}", () => new List<string>());
                var locloc = rawData
                    .Select(item => item.Replace(',', '.')) // Zamiana przecinków na kropki
                    .Where(item => double.TryParse(item, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _)) 
                    .Select(item => double.Parse(item, System.Globalization.CultureInfo.InvariantCulture)) 
                    .ToList();
                return locloc;
            }

            // Jeśli defaultLoc jest `null` lub puste, zwracamy `null`
            return null;
        }

        public static string? GetAPIKey(string which)
        {
            // Klucz mapujący rodzaj API na odpowiedni klucz w danych
            var keyMap = new Dictionary<string, string>
    {
        { "astro", "MOONAPIkey" },
        { "weather", "APIkey" }
    };

            // Sprawdzamy, czy podano prawidłowy typ API
            if (!keyMap.TryGetValue(which, out string? dataKey))
            {
                Console.WriteLine($"Invalid API type: {which}");
                return null;
            }

            // Ładujemy listę kluczy API z pliku
            var apiKeys = LogFileGetSet.LoadData(dataKey, () => new List<string>());

            // Sprawdzamy, czy lista zawiera przynajmniej jeden element
            if (apiKeys != null && apiKeys.Count > 0)
            {
                return apiKeys[0];
            }

            Console.WriteLine($"No API key found for {which}");
            return null;
        }

    }
}
