using System.IO;
using Plugin.Maui.Audio;
using CommunityToolkit.Maui.Views;
using Circle.Pages;
using CommunityToolkit.Maui.Core.Primitives;

namespace Circle;

public partial class MainPage : ContentPage
{


    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        //初始化音频进内存备用
        await SoundManager.InitializeAsync();

        // 等待一小会儿，确保安卓视图渲染完毕
        await Task.Delay(100);

        // 1. 标题飞入动画
        await Task.WhenAll(
            CircleLabel.TranslateToAsync(0, 0, 1500, Easing.CubicOut),
            CircleLabel.FadeToAsync(1, 1500, Easing.CubicOut)
        );

        // 2. 按钮淡入动画
        await StartButton.FadeToAsync(1, 1000, Easing.Linear);

        StartButton.IsEnabled = true;
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false; // 防止重复点击

        SoundManager.PlayClick();

        // 按钮缩放反馈效果
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        // 稍微等待声音开始播放
        await Task.Delay(200);

        await Navigation.PushAsync(new ChooseCharacter());

        button.IsEnabled = true;
    }
}