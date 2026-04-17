namespace Circle.Pages;

public partial class VictoryPage : ContentPage
{
    public VictoryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ensure the view has finished rendering
        await Task.Delay(100);

        // Play animation
        await Task.WhenAll(
            VictoryLabel.TranslateToAsync(0, 0, 1500, Easing.CubicOut),
            VictoryLabel.FadeToAsync(1, 1500, Easing.CubicOut)
        );

        await Task.WhenAll(
            RestartButton.FadeToAsync(1, 1000, Easing.Linear),
            MenuButton.FadeToAsync(1, 1000, Easing.Linear)
        );

        RestartButton.IsEnabled = true;
        MenuButton.IsEnabled = true;
    }

    private async void OnRestartClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        await RestartButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await RestartButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // Navigate to the character selection page
        await Navigation.PushAsync(new ChooseCharacter());
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        await MenuButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await MenuButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // Clear the stack directly and return to the main menu
        await Navigation.PopToRootAsync();
    }
}