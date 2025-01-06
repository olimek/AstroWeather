

using AstroWeather.Helpers;

namespace AstroWeather
{
    
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";
            LogFileGetSet.StoreData<List<string>>("hello", new List<string>() { "ASD", "AAA" });

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
