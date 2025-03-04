namespace AstroWeather.Pages
{
    public partial class PopUp : ContentPage
    {
        public PopUp(string labelText)
        {
            InitializeComponent();
            ((Label)this.Content.FindByName("popupLabel")).Text = labelText;

            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, EventArgs e)
        {
            this.Loaded -= Page_Loaded;

            PoppingIn();

            AutoClose();
        }

        public void PoppingIn()
        {
            var contentSize = this.Content.Measure(Window.Width, Window.Height, MeasureFlags.IncludeMargins);
            var contentHeight = contentSize.Request.Height;
            this.Content.TranslationY = contentHeight;
            this.Animate("Background",
                callback: v => this.Background = new SolidColorBrush(Colors.Black.WithAlpha((float)v)),
                start: 0d,
                end: 0.7d,
                rate: 32,
                length: 350,
                easing: Easing.CubicOut,
                finished: (v, k) =>
                    this.Background = new SolidColorBrush(Colors.Black.WithAlpha(0.7f)));

            this.Animate("Content",
                callback: v => this.Content.TranslationY = (int)(contentHeight - v),
                start: 0,
                end: contentHeight,
                length: 500,
                easing: Easing.CubicInOut,
                finished: (v, k) => this.Content.TranslationY = 0);
        }

        public Task PoppingOut()
        {
            var done = new TaskCompletionSource<bool>();

            if (this.Content == null)
            {
                done.TrySetResult(true);
                return done.Task;
            }
            double pageWidth = this.Width;
            double pageHeight = this.Height;

            if (double.IsNaN(pageWidth) || double.IsNaN(pageHeight) || pageWidth <= 0 || pageHeight <= 0)
            {
                pageWidth = 300;
                pageHeight = 600;
            }

            var contentSize = this.Content.Measure(pageWidth, pageHeight, MeasureFlags.IncludeMargins);
            double windowHeight = contentSize.Request.Height;

            if (windowHeight <= 0)
            {
                windowHeight = pageHeight;
            }

            this.Animate(
                name: "Background",
                callback: v => this.Background = new SolidColorBrush(Colors.Black.WithAlpha((float)v)),
                start: 0.7d,
                end: 0d,
                rate: 32,
                length: 350,
                easing: Easing.CubicIn,
                finished: (v, k) => this.Background = new SolidColorBrush(Colors.Black.WithAlpha(0.0f))
            );

            this.Animate(
                name: "Content",
                callback: v =>
                {
                    if (this.Content != null)
                    {
                        this.Content.TranslationY = (int)(windowHeight - v);
                    }
                },
                start: windowHeight,
                end: 0,
                length: 500,
                easing: Easing.CubicInOut,
                finished: (v, k) =>
                {
                    if (this.Content != null)
                        this.Content.TranslationY = windowHeight;

                    done.TrySetResult(true);
                }
            );

            return done.Task;
        }

        private async void TapGestureRecognizer_OnTapped(object sender, TappedEventArgs e)
        {
            await Close();
        }

        private async Task Close()
        {
            await PoppingOut();
            await Navigation.PopModalAsync(animated: false);
        }

        private async void AutoClose()
        {
            await Task.Delay(2000);
            await Close();
        }
    }
}