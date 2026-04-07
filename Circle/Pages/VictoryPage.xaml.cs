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

        // 确保视图渲染完毕
        await Task.Delay(100);

        // 播放动画
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

        // 跳转到选择角色页面
        await Navigation.PushAsync(new ChooseCharacter());
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        await MenuButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await MenuButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // 直接清空栈，回到主菜单
        await Navigation.PopToRootAsync();
    }
}