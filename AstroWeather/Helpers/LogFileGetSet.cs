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
                // Load existing data or create a new dictionary
                Dictionary<string, T> data;

                string filePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Current.Name), "data.json");

                if (!Directory.Exists(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        AppInfo.Current.Name)))

                {
                    Directory.CreateDirectory(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        AppInfo.Current.Name));
                }

                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }
                data = new Dictionary<string, T>();
                data[key] = value;

                // Add or update the value associated with the key

                // Serialize and save the updated data to the file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    string json = JsonSerializer.Serialize(data);
                    writer.Write(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save data: {ex.Message}");
            }
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
