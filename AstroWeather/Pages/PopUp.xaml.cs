using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AstroWeather.Pages
{
    public partial class PopUp : ContentPage
    {
        public PopUp(string labelText)
        {
            InitializeComponent();

            // Set the label text
            ((Label)this.Content.FindByName("popupLabel")).Text = labelText;

            this.Loaded += Page_Loaded;
        }

        void Page_Loaded(object sender, EventArgs e)
        {
            // We only need this to fire once, so clean things up!
            this.Loaded -= Page_Loaded;

            // Call the animation
            PoppingIn();

            // Auto-close after 2 seconds
            AutoClose();
        }

        public void PoppingIn()
        {
            // Measure the actual content size
            var contentSize = this.Content.Measure(Window.Width, Window.Height, MeasureFlags.IncludeMargins);
            var contentHeight = contentSize.Request.Height;

            // Start by translating the content below / off screen
            this.Content.TranslationY = contentHeight;

            // Animate the translucent background, fading into view
            this.Animate("Background",
                callback: v => this.Background = new SolidColorBrush(Colors.Black.WithAlpha((float)v)),
                start: 0d,
                end: 0.7d,
                rate: 32,
                length: 350,
                easing: Easing.CubicOut,
                finished: (v, k) =>
                    this.Background = new SolidColorBrush(Colors.Black.WithAlpha(0.7f)));

            // Also animate the content sliding up from below the screen
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
            // Najlepiej użyć TaskCompletionSource<bool>, aby zwrócić Task<bool>
            var done = new TaskCompletionSource<bool>();

            // Jeśli Content jest null, przerywamy animację, by uniknąć NullReferenceException
            if (this.Content == null)
            {
                done.TrySetResult(true);
                return done.Task;
            }

            // Pobieramy wymiary bieżącej strony (lub kontrolki)
            double pageWidth = this.Width;
            double pageHeight = this.Height;

            // Jeśli strona nie została jeszcze zmierzona, wymiary mogą być 0 lub NaN
            // Ustawiamy awaryjnie np. 300x600
            if (double.IsNaN(pageWidth) || double.IsNaN(pageHeight) || pageWidth <= 0 || pageHeight <= 0)
            {
                pageWidth = 300;
                pageHeight = 600;
            }

            // Mierzymy rozmiar Content
            var contentSize = this.Content.Measure(pageWidth, pageHeight, MeasureFlags.IncludeMargins);
            double windowHeight = contentSize.Request.Height;

            // Jeśli pomiar dał 0, ustaw awaryjnie na wysokość strony
            if (windowHeight <= 0)
            {
                windowHeight = pageHeight;
            }

            // Animacja "Background": wygaszanie tła
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

            // Animacja "Content": przesuwanie zawartości w dół
            this.Animate(
                name: "Content",
                callback: v =>
                {
                    if (this.Content != null)
                    {
                        // (windowHeight - v) będzie zmniejszać się od windowHeight do 0
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

                    // Zwracamy kontrolę – animacja się skończyła
                    done.TrySetResult(true);
                }
            );

            return done.Task;
        }


        private async void TapGestureRecognizer_OnTapped(object sender, TappedEventArgs e)
        {
            await Close();
        }

        async Task Close()
        {
            // Wait for the animation to complete
            await PoppingOut();
            // Navigate away without the default animation
            await Navigation.PopModalAsync(animated: false);
        }

        async void AutoClose()
        {
            await Task.Delay(2000); // Wait for 2 seconds
            await Close(); // Close the popup
        }
    }
}