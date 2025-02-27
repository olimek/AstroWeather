using Microcharts.Maui;
using Microsoft.Extensions.Logging;
#if WINDOWS
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
#endif
using Plugin.LocalNotification;

namespace AstroWeather
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMicrocharts()
                .UseLocalNotification(); // Rejestracja Plugin.LocalNotification

#if WINDOWS
            try
            {
                // Najpierw rejestrujemy obsługę zdarzeń powiadomień
                AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

                // Dopiero potem rejestrujemy powiadomienia
                AppNotificationManager.Default.Register();

                System.Diagnostics.Debug.WriteLine("🔔 Windows: AppNotificationManager zarejestrowany.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Błąd rejestracji powiadomień Windows: {ex.Message}");
            }
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

#if WINDOWS
        private static void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
        {
            // Obsługa powiadomienia systemowego Windows (toast).
            // Możesz wyciągnąć parametry z args.Argument i zareagować w aplikacji.
            System.Diagnostics.Debug.WriteLine($"🔔 Windows NotificationInvoked: {args.Argument}");
        }
#endif
    }
}
