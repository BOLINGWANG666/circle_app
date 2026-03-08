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

        // 确保视图渲染完毕
        await Task.Delay(100);

        // 1. 标题飞入动画 (和 MainPage 类似)
        await Task.WhenAll(
            FailedLabel.TranslateToAsync(0, 0, 1500, Easing.CubicOut),
            FailedLabel.FadeToAsync(1, 1500, Easing.CubicOut)
        );

        // 2. 按钮淡入动画
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
        // 点击效果
        await RestartButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await RestartButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // 跳转到选择角色页面
        await Navigation.PushAsync(new ChooseCharacter());
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        SoundManager.PlayClick();
        // 点击效果
        await MenuButton.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await MenuButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // 使用 PopToRootAsync 直接清空所有叠加的页面，返回最初的 MainPage
        // 这样可以避免游戏玩久了 Navigation 堆栈爆满导致内存泄漏
        await Navigation.PopToRootAsync();
    }
}