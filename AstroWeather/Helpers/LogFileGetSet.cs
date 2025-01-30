using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace AstroWeather.Helpers
{
    public class LogFileGetSet
    {
        public string GetFilePath(string fileName)
        {
            string documentsPath = FileSystem.AppDataDirectory;
            return Path.Combine(documentsPath, fileName);
        }

        public bool FileExists(string fileName)
        {
            string filePath = GetFilePath(fileName);
            return File.Exists(filePath);
        }

        public async Task SaveFileAsync(string content, string fileName)
        {
            string filePath = GetFilePath(fileName);
            using (var writer = new StreamWriter(filePath, false))
            {
                await writer.WriteAsync(content);
            }
        }

        public async Task<string> ReadFileAsync(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            return null;
        }

        public async Task StoreDataAsync<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.");
                if (value == null) throw new ArgumentNullException(nameof(value));

                string fileName = "data.json";
                string filePath = GetFilePath(fileName);

                Dictionary<string, T> data;
                if (FileExists(fileName))
                {
                    string existingJson = await ReadFileAsync(fileName);
                    data = JsonSerializer.Deserialize<Dictionary<string, T>>(existingJson) ?? new Dictionary<string, T>();
                }
                else
                {
                    data = new Dictionary<string, T>();
                }

                data[key] = value;

                await WriteDataAsync(filePath, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing data: {ex.Message}");
            }
        }

        private async Task WriteDataAsync<T>(string filePath, Dictionary<string, T> data)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task RemoveDataAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.");

                string fileName = "data.json";
                string filePath = GetFilePath(fileName);

                if (!FileExists(fileName))
                {
                    Console.WriteLine("Data file not found.");
                    return;
                }

                string existingJson = await ReadFileAsync(fileName);
                var data = JsonSerializer.Deserialize<Dictionary<string, T>>(existingJson) ?? new Dictionary<string, T>();

                if (data.ContainsKey(key))
                {
                    data.Remove(key);
                    await WriteDataAsync(filePath, data);
                    Console.WriteLine($"Successfully removed key: {key}");
                }
                else
                {
                    Console.WriteLine($"Key '{key}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing data: {ex.Message}");
            }
        }

        public async Task<T> LoadDataAsync<T>(string key, Func<T> defaultFactory)
        {
            try
            {
                string fileName = "data.json";
                string filePath = GetFilePath(fileName);

                if (FileExists(fileName))
                {
                    string json = await ReadFileAsync(fileName);
                    var rootObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (rootObject != null && rootObject.TryGetValue(key, out JsonElement valueElement))
                    {
                        if (valueElement.ValueKind == JsonValueKind.Array || valueElement.ValueKind == JsonValueKind.String)
                        {
                            return valueElement.Deserialize<T>();
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

        public async Task<Dictionary<string, object>> LoadAllDataAsync()
        {
            try
            {
                string fileName = "data.json";
                string filePath = GetFilePath(fileName);

                if (FileExists(fileName))
                {
                    string json = await ReadFileAsync(fileName);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    return data ?? new Dictionary<string, object>();
                }

                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load all data: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        public async Task<List<double>?> LoadDefaultLocAsync()
        {
            var defaultLoc = await LoadDataAsync("DefaultLoc", () => new List<string>());

            if (defaultLoc != null && defaultLoc.Count > 0)
            {
                var rawData = await LoadDataAsync($"Localisation_{defaultLoc[0]}", () => new List<string>());
                var locloc = rawData
                    .Select(item => item.Replace(',', '.'))
                    .Where(item => double.TryParse(item, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                    .Select(item => double.Parse(item, System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();
                return locloc;
            }

            return null;
        }

        public async Task<string?> LoadDefaultLocNameAsync()
        {
            var defaultLoc = await LoadDataAsync("DefaultLoc", () => new List<string>());

            if (defaultLoc != null && defaultLoc.Count > 0)
            {
                return defaultLoc[0];
            }

            return null;
        }

        public async Task<string?> GetAPIKeyAsync(string which)
        {
            var keyMap = new Dictionary<string, string>
            {
                { "astro", "MOONAPIkey" },
                { "weather", "APIkey" }
            };

            if (!keyMap.TryGetValue(which, out string? dataKey))
            {
                Console.WriteLine($"Invalid API type: {which}");
                return null;
            }

            var apiKeys = await LoadDataAsync(dataKey, () => new List<string>());

            if (apiKeys != null && apiKeys.Count > 0)
            {
                return apiKeys[0];
            }

            Console.WriteLine($"No API key found for {which}");
            return null;
        }
    }
}