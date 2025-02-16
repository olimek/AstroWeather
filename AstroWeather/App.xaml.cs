using System.ComponentModel.Design;
using AstroWeather.Helpers;
using AstroWeather.Pages;
using CosineKitty;


namespace AstroWeather

{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {

            var window = new Window(new AppShell());

            window.Activated += Window_Activated;

            return window;
        }


        private static void Window_Activated(object? sender, EventArgs e)
        {
#if WINDOWS
            const int DefaultWidth = 600;
            const int DefaultHeight = 800;

            if (sender is Window window)
            {
                // zmiana rozmiaru okna
                window.Width = DefaultWidth;
                window.Height = DefaultHeight;

                // daj trochę czasu na zakończenie zadania zmiany rozmiaru okna
                window.Dispatcher.Dispatch(() => { });

                var disp = DeviceDisplay.Current.MainDisplayInfo;

                // przeniesienie okna do centrum ekranu
                window.X = (disp.Width / disp.Density - window.Width) / 2;
                window.Y = (disp.Height / disp.Density - window.Height) / 2;
            }
#endif
        }
    }
}