// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Minor Code Smell", "S2386:Mutable fields should not be \"public static\"", Justification = "<Pending>", Scope = "type", Target = "~T:AstroWeather.MainPage")]
[assembly: SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "<Pending>", Scope = "member", Target = "~F:AstroWeather.MainPage.GlobalWeatherList")]
[assembly: SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "<Pending>", Scope = "member", Target = "~F:AstroWeather.MainPage.GlobalWeatherList")]
[assembly: SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>", Scope = "member", Target = "~F:AstroWeather.MainPage.GlobalWeatherList")]
[assembly: SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "<Pending>", Scope = "member", Target = "~M:AstroWeather.MainPage.MainInit~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Performance", "XC0022:Binding could be compiled to improve runtime performance if x:DataType is specified.", Justification = "<Pending>", Scope = "member", Target = "~M:AstroWeather.Pages.SettingsPage.InitializeComponent")]