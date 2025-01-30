using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace AstroWeather.Helpers
{
    public class FileService
    {
        private readonly string _fileName = "example.txt";

        public string GetFilePath()
        {
            string documentsPath = FileSystem.AppDataDirectory;
            return Path.Combine(documentsPath, _fileName);
        }

        public async Task SaveFileAsync(string content)
        {
            string filePath = GetFilePath();
            using (var writer = new StreamWriter(filePath, false))
            {
                await writer.WriteAsync(content);
            }
        }

        public async Task<string> ReadFileAsync()
        {
            string filePath = GetFilePath();
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            return null;
        }

        public bool FileExists()
        {
            string filePath = GetFilePath();
            return File.Exists(filePath);
        }
    }
}