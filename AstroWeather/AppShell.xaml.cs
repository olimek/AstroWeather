using AstroWeather.Pages;
namespace AstroWeather
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            
            InitializeComponent();
            if (false)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("//MainPage");
                });

            }
            else
            {
                Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("//SettingsPage");
                });

            }
            
        }
    }
}
