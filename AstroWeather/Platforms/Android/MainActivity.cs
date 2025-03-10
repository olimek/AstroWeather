using Android.App;
using Android.Content.PM;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Maui;

namespace AstroWeather
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public override void OnBackPressed()
        {
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var currentPage = Shell.Current?.CurrentPage;

                if (currentPage is not MainPage) // Jeśli użytkownik nie jest na MainPage
                {
                    await Shell.Current?.GoToAsync("//MainPage"); // Przejdź do MainPage
                }
                else
                {
                    // Przenieś aplikację do tła zamiast zamykania
                    MoveTaskToBack(true);
                }
            });
        }
    }
}
