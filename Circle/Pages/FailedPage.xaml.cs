namespace Circle.Pages;

public partial class FailedPage : ContentPage
{
    public FailedPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ensure the view has finished rendering
        await Task.Delay(100);


        await Task.WhenAll(
            FailedLabel.TranslateToAsync(0, 0, 1500, Easing.CubicOut),
            FailedLabel.FadeToAsync(1, 1500, Easing.CubicOut)
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
        // Click effect
        await RestartButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await RestartButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // Navigate to the character selection page
        await Navigation.PushAsync(new ChooseCharacter());
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        // Click effect
        await MenuButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await MenuButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // Use PopToRootAsync to clear all stacked pages directly and return to the initial MainPage
        // This avoids memory leaks caused by a full Navigation stack after playing the game for a long time
        await Navigation.PopToRootAsync();
    }
}