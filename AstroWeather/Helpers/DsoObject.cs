﻿using System;
using System.Collections.Generic;
using System.IO;
using CosineKitty;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class DsoObject
{
    public string Constellation { get; set; }
    public string Dec { get; set; }
    public string Description { get; set; }
    public double Mag { get; set; }
    public string Name { get; set; }
    public string Ra { get; set; }
    public double Size { get; set; }
    public string Type { get; set; }
    public double MaxAlt { get; set; }   // Maksymalna wysokość (w stopniach)
    public double Visible { get; set; }  // Procent widoczności w ciągu nocy
}

public class DsoCalculator
{
    private List<DsoObject> _dsoObjects;

    public DsoCalculator(string filePath)
    {
        _dsoObjects = LoadDsoData(filePath);
    }

    public static List<DsoObject> LoadDsoData(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yamlText = File.ReadAllText(filePath);
       
        return deserializer.Deserialize<List<DsoObject>>(yamlText);
    }


    public static double ParseRA(string raStr)
    {
        var parts = raStr.Split(' ');
        if (parts.Length != 3)
            throw new FormatException("RA powinno mieć format HH:MM:SS");

        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        int seconds = int.Parse(parts[2]);

        return hours + (minutes / 60.0) + (seconds / 3600.0); // Wynik w godzinach (0-24)
    }

    public static double ParseDec(string decStr)
    {
        var parts = decStr.Split(' ');
        if (parts.Length != 3)
            throw new FormatException("Dec powinno mieć format ±DD:MM:SS");

        int degrees = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        int seconds = int.Parse(parts[2]);

        double sign = degrees < 0 ? -1 : 1;
        double absDegrees = Math.Abs(degrees) + (minutes / 60.0) + (seconds / 3600.0);

        return sign * absDegrees; // Wynik w stopniach (-90 do 90)
    }


    public static DsoObject CalculateVisibilityAndAltitude(DsoObject dso, DateTime dateUtc, List<DateTime> astrotimes, double lat, double lon)
    {
        
        double observerLatitude = 51.108;
        double observerLongitude = 17.0385;
        Observer observer = new Observer(observerLatitude, observerLongitude, 100);

        double raRad = ParseRA(dso.Ra);
        double decRad = ParseDec(dso.Dec);

        
        Astronomy.DefineStar(Body.Star1, raRad, decRad, 1000);

        DateTime sunset = astrotimes[0];
        DateTime sunrise = astrotimes[1]; 

        double visibleTime = 0;
        double maxAlt = double.MinValue;

        
        TimeSpan step = TimeSpan.FromMinutes(5);
        for (DateTime currentTime = sunset; currentTime <= sunrise; currentTime += step)
        {
            AstroTime astroCurrent = new AstroTime(currentTime);
            Equatorial eq = Astronomy.Equator(Body.Star1, astroCurrent, observer, EquatorEpoch.OfDate, Aberration.Corrected);
            Topocentric topo = Astronomy.Horizon(astroCurrent, observer, eq.ra, eq.dec, Refraction.Normal);

            if (topo.altitude > maxAlt)
                maxAlt = topo.altitude;

            if (topo.altitude > 20) 
                visibleTime += step.TotalHours;
        }

        
        double totalNight = (sunrise - sunset).TotalHours;
        dso.Visible = Math.Min((visibleTime / totalNight) * 100.0, 100.0);
        dso.MaxAlt = maxAlt;
        return dso;
    }

    public List<DsoObject> GetTopVisibleObjects(DateTime dateUtc, List<DateTime> astrotimes, int count, double lat, double lon)
    {
        var calculatedDsoObjects = _dsoObjects
            .Where(dso => dso.Mag != -9999) // Ignorowanie obiektów z nieznaną jasnością
            .Select(dso => CalculateVisibilityAndAltitude(dso, dateUtc, astrotimes,lat,lon))
            .ToList();

        return calculatedDsoObjects
            .OrderByDescending(dso => dso.Visible)
            .ThenBy(dso => dso.Mag) // Sortowanie według jasności (magnitude) rosnąco
            .Take(count)
            .ToList();
    }
}