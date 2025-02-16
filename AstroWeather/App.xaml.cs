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
            Task<bool> task = RequestStoragePermissionAsync();
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = base.CreateWindow(activationState);
            window.Activated += Window_Activated;
            return window;
        }

        public async Task<bool> RequestStoragePermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return status == PermissionStatus.Granted;
        }

        private static async void Window_Activated(object? sender, EventArgs e)
        {
#if WINDOWS
            const int DefaultWidth = 600;
            const int DefaultHeight = 800;

            if (sender is Window window)
            {
                // change window size.
                window.Width = DefaultWidth;
                window.Height = DefaultHeight;

                // give it some time to complete window resizing task.
                await Task.Run(() => window.Dispatcher.Dispatch(() => { }));

                var disp = DeviceDisplay.Current.MainDisplayInfo;

                // move to screen center
                window.X = (disp.Width / disp.Density - window.Width) / 2;
                window.Y = (disp.Height / disp.Density - window.Height) / 2;
            }
#endif
        }
    }
}