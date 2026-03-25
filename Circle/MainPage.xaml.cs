using Circle.Pages;
using Circle.Models;

namespace Circle;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        if (Circle.ViewModels.CharacterViewModel.Current == null)
        {
            new Circle.ViewModels.CharacterViewModel();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SoundManager.InitializeAsync();
        await Task.Delay(100);

        var allSaves = Circle.ViewModels.CharacterViewModel.Current?.Characters;

        if (allSaves != null && allSaves.Count > 0)
        {
            SaveFileButton.IsVisible = true;
        }
        else
        {
            SaveFileButton.IsVisible = false;
        }

       
        UpdateButtonsLayout();

        // 飞入动画
        await Task.WhenAll(
            CircleLabel.TranslateToAsync(0, 0, 1500, Easing.CubicOut),
            CircleLabel.FadeToAsync(1, 1500, Easing.CubicOut)
        );

        
        var fadeTasks = new List<Task>();
        if (StartButton.IsVisible) fadeTasks.Add(StartButton.FadeToAsync(1, 1000, Easing.Linear));
        if (SaveFileButton.IsVisible) fadeTasks.Add(SaveFileButton.FadeToAsync(1, 1000, Easing.Linear));
        if (HowToPlayButton.IsVisible) fadeTasks.Add(HowToPlayButton.FadeToAsync(1, 1000, Easing.Linear));

        await Task.WhenAll(fadeTasks);

        StartButton.IsEnabled = true;
        HowToPlayButton.IsEnabled = true;
        SaveFileButton.IsEnabled = true;
    }

    
    private void UpdateButtonsLayout()
    {
        // 收集当前需要显示的所有按钮
        var visibleButtons = new List<Button>();

        if (StartButton.IsVisible) visibleButtons.Add(StartButton);
        if (SaveFileButton.IsVisible) visibleButtons.Add(SaveFileButton);
        if (HowToPlayButton.IsVisible) visibleButtons.Add(HowToPlayButton);

        // 根据数量分配网格坐标
        if (visibleButtons.Count == 1)
        {
            // 如果只有 1 个：放在第 0 行，横跨两列居中
            Grid.SetRow(visibleButtons[0], 0);
            Grid.SetColumn(visibleButtons[0], 0);
            Grid.SetColumnSpan(visibleButtons[0], 2);
        }
        else if (visibleButtons.Count == 2)
        {
            // 如果有 2 个：放在第 0 行，一人占一列并排
            Grid.SetRow(visibleButtons[0], 0);
            Grid.SetColumn(visibleButtons[0], 0);
            Grid.SetColumnSpan(visibleButtons[0], 1);

            Grid.SetRow(visibleButtons[1], 0);
            Grid.SetColumn(visibleButtons[1], 1);
            Grid.SetColumnSpan(visibleButtons[1], 1);
        }
        else if (visibleButtons.Count == 3)
        {
            // 如果有 3 个：前两个放在第 0 行并排，第 3 个放在第 1 行并横跨两列居中
            Grid.SetRow(visibleButtons[0], 0);
            Grid.SetColumn(visibleButtons[0], 0);
            Grid.SetColumnSpan(visibleButtons[0], 1);

            Grid.SetRow(visibleButtons[1], 0);
            Grid.SetColumn(visibleButtons[1], 1);
            Grid.SetColumnSpan(visibleButtons[1], 1);

            Grid.SetRow(visibleButtons[2], 1);
            Grid.SetColumn(visibleButtons[2], 0);
            Grid.SetColumnSpan(visibleButtons[2], 2);
        }
    }

   
    private async void OnStartClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();

        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);
        await Task.Delay(200);

        await Navigation.PushAsync(new ChooseCharacter());
        button.IsEnabled = true;
    }

    private async void OnSaveFileClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        await Task.Delay(150);

        await Navigation.PushAsync(new SaveFilesPage());

        button.IsEnabled = true;
    }

    private async void OnHowToPlayClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        await Navigation.PushAsync(new Circle.Pages.HowToPlayPage());

        button.IsEnabled = true;
    }
}