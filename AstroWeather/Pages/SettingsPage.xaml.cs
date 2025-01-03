using System.Windows.Input;

namespace AstroWeather.Pages;

public partial class SettingsPage : ContentPage
{
    public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
    public SettingsPage()
	{
        InitializeComponent();
        BindingContext = this;
    }

    void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        string oldText = e.OldTextValue;
        string newText = e.NewTextValue;
        string myText = APIkeyInput.Text;
    }
    void OnEntryCompleted(object sender, EventArgs e)
    {
        string text = ((Entry)sender).Text;
    }
}