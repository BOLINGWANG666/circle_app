using System.IO;

namespace Circle;


using Circle.Pages;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 在代码后置中显式加载资源，确保它是从 MauiAsset 中加载
            AudioPlayer.Source = MediaSource.FromResource("tapstart.mp3");

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

    private void OnMediaFailed(object? sender, EventArgs e)
    {
        // 简化处理器，避免命名空间问题
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false; // 防止重复点击
        
        AudioPlayer.Stop();
        AudioPlayer.Play();

        // 按钮缩放反馈效果
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);
        
        // 稍微等待声音开始播放
        await Task.Delay(200);

        await Navigation.PushAsync(new ChooseCharacter());

        button.IsEnabled = true;
    }


    }

