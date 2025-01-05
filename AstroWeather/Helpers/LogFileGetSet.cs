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
                // Load existing data or return the default value
                Dictionary<string, T> data;
                string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.Name), "data.json");
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string json = reader.ReadToEnd();
                        data = JsonSerializer.Deserialize<Dictionary<string, T>>(json);

                        if (data != null)
                        {
                            if (data.ContainsKey(key))
                            {
                                return data[key];
                            }
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
