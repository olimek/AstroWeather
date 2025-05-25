using System.Diagnostics;
using AstroWeather.Helpers;
using CosineKitty;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace AstroWeather
{
    public partial class MainPage : ContentPage
    {
        public static List<AstroWeather.Helpers.Day> GlobalWeatherList = [];
        private static DateTime LastWeatherUpdateTime = DateTime.MinValue;

        private System.Timers.Timer weatherTimer;

        public MainPage()
        {
            InitializeComponent();
            LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationTapped;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            weatherTimer = new System.Timers.Timer(TimeSpan.FromHours(3).TotalMilliseconds);
            weatherTimer.Elapsed += async (s, e) =>
            {
                Dispatcher.Dispatch(async () =>
                {
                    await RefreshWeatherIfNeeded();
                });
            };
            weatherTimer.AutoReset = true;
            weatherTimer.Start();
            await RefreshWeatherIfNeeded();
            await DrawMoonGraph(0);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (weatherTimer != null)
            {
                weatherTimer.Stop();
                weatherTimer.Dispose();
                weatherTimer = null;
            }
            LocalNotificationCenter.Current.NotificationActionTapped -= OnNotificationTapped;
        }

        private void OnNotificationTapped(NotificationActionEventArgs e)
        {
            Debug.WriteLine($"🔔 Kliknięto powiadomienie: {e.Request.Title}");
            Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("//WeatherPage");
            });
        }

        private async Task RefreshWeatherIfNeeded()
        {
            if ((DateTime.UtcNow - LastWeatherUpdateTime).TotalHours >= 1)
            {
                await MainInit();
            }
            else
            {
                if (GlobalWeatherList!.Count <= 3)
                {
                    await MainInit();
                }
                else
                {
                    BindingContext = new { weather = GlobalWeatherList };
                }
            }
        }

        private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            int firstVisibleIndex = e.FirstVisibleItemIndex;
            _ = DrawMoonGraph(firstVisibleIndex);
        }

        private async Task DrawMoonGraph(int firstVisibleIndex)
        {
            if (GlobalWeatherList.Count <= 3)
            {
                await Shell.Current.GoToAsync("//SettingsPage");
            }
            else
            {
                DateTime.TryParseExact(GlobalWeatherList[firstVisibleIndex].datetime, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime parsedDate);

                var time = new AstroTime(parsedDate);
                double phase = Astronomy.MoonPhase(time);

                IllumInfo illum = Astronomy.Illumination(Body.Moon, time);
                MoonImage.Source = WeatherRouter.GetMoonImage(Math.Round(100.0 * illum.phase_fraction), phase);

                var moontimes = WeatherRouter.GetAstroTimes(parsedDate, true);
                var moonSet = moontimes[4];
                var moonrise = moontimes[5];
                TimeSpan nightDuration = moontimes[3] - moontimes[2];

                ActualTemp.Text = $"Data: {parsedDate:dd-MM}, Długość nocy: {Math.Round(nightDuration.TotalHours, 1)}";
                ActualHum.Text = $"Moonrise: {moonrise:HH:mm dd-MM}, Moonset: {moonSet:HH:mm dd-MM}";
                Actualpress.Text = $"Moon illumination: {Math.Round(100.0 * illum.phase_fraction, 1)} %";
            }
        }

        public async Task ShowNotification(string title, string message)
        {
            await Task.Delay(1000 * 60);
            try
            {
#if ANDROID
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
                {
                    if (Platform.CurrentActivity?.CheckSelfPermission(Android.Manifest.Permission.PostNotifications)
                        != Android.Content.PM.Permission.Granted)
                    {
                        Platform.CurrentActivity?.RequestPermissions(
                            new string[] { Android.Manifest.Permission.PostNotifications }, 0);
                    }
                }
#endif
                var notification = new NotificationRequest
                {
                    NotificationId = 1000,
                    Title = title,
                    Description = message,
                    ReturningData = "weather_update",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Now.AddSeconds(5)
                    }
                };

                await LocalNotificationCenter.Current.Show(notification);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Błąd powiadomienia: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Błąd",
                    $"Nie można wysłać powiadomienia: {ex.Message}", "OK");
            }
        }

        private async Task MainInit()
        {
            if (!WeatherRouter.IsApiVariables())
            {
                await Dispatcher.DispatchAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//SettingsPage");
                });
            }
            else
            {
                //await Navigation.PushModalAsync(new AstroWeather.Pages.PopUp("Pobieranie pogody"));
                var pogodaDzienna = await WeatherRouter.SetWeatherBindingContextAsync()!;
                GlobalWeatherList = pogodaDzienna!.ToList()!;
                LastWeatherUpdateTime = DateTime.UtcNow!;
                BindingContext = new { weather = GlobalWeatherList };

                var defaultLocName = await LogFileGetSet.LoadDefaultLocNameAsync();
                SecondLabel.Text = defaultLocName ?? "Default location name not found";

                var dobryDzien = GlobalWeatherList.FirstOrDefault(d => d.astrocond > 50);
                if (dobryDzien != null)
                {
                    Debug.WriteLine($"🔔 Powiadomienie: Znaleziono dobry dzień {dobryDzien.datetime}, astrocond: {dobryDzien.astrocond}");
                    await ShowNotification("Dobre warunki do astrofotografii!",
                        $"Dzień {dobryDzien.datetime} ma warunki {dobryDzien.astrocond}%.");
                }

                await Shell.Current.GoToAsync("//MainPage");
            }
        }

        private static async void WeatherListView_ItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection!.FirstOrDefault() is AstroWeather.Helpers.Day selectedItem)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "selectedDay", selectedItem }
                };

                await Shell.Current.GoToAsync("//WeatherPage", parameters);
            }
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}