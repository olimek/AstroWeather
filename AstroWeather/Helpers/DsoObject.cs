using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// Klasa reprezentująca pojedynczy obiekt DSO
public class DsoObject
{
    public string constellation { get; set; }
    public string dec { get; set; }
    public string description { get; set; }
    public double mag { get; set; }
    public string name { get; set; }
    public string ra { get; set; }
    public double size { get; set; }
    public string type { get; set; }
}

public class DsoYamlLoader
{
    public static List<DsoObject> LoadDsoData(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yamlText = File.ReadAllText(filePath);
        return deserializer.Deserialize<List<DsoObject>>(yamlText);
    }

    public static DsoObject GetDsoByName(string name, List<DsoObject> dsoList)
    {
        return dsoList.FirstOrDefault(d => d.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
