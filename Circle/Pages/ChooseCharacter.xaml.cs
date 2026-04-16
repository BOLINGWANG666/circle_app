using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes; // 必须引入以支持 Polygon 类型

namespace Circle.Pages;

public partial class ChooseCharacter : ContentPage
{
    private int _selectedIndex = 0; // 0=未选, 1=圆形, 2=三角形

    private int _selectedHp;
    private int _selectedAtk;
    private double _selectedCd;
    private double _selectedDodge;
    private Color _selectedColor = Colors.Transparent;

    public ChooseCharacter()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (AudioPlayer != null)
        {
            AudioPlayer.Source = MediaSource.FromResource("BottonSound.mp3"); 
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await BackButton.ScaleToAsync(0.9, 100, Easing.CubicOut);
        await BackButton.ScaleToAsync(1.0, 100, Easing.CubicIn);
        await Navigation.PopAsync();
    }

    // 角色 1 (圆形) 的点击事件
    private async void OnCharacter1Tapped(object sender, EventArgs e)
    {
        // 判断是否已经是选中状态
        if (_selectedIndex == 1)
        {
            // 如果点的是已选中的，则取消选中
            _selectedIndex = 0;
            SelectionBorder1.IsVisible = false;
            StatsPanel.IsVisible = false;
            _selectedColor = Colors.Transparent;
        }
        else
        {
            // 否则，执行选中逻辑
            _selectedIndex = 1;
            SelectionBorder1.IsVisible = true;
            SelectionBorder2.IsVisible = false;
            StatsPanel.IsVisible = true;
            WarningLabel.IsVisible = false;

            _selectedHp = 50;
            _selectedAtk = 15;
            _selectedCd = 1.2;
            _selectedDodge = 0.05;
            _selectedColor = Colors.Gray;

            UpdateDisplay();
        }

        // 无论选中还是取消，都播放一个缩放反馈
        if (sender is View clickedShape)
        {
            await clickedShape.ScaleToAsync(1.1, 100, Easing.CubicOut);
            await clickedShape.ScaleToAsync(1.0, 100, Easing.CubicIn);
        }
    }

    // 角色 2 (三角形) 的点击事件
    private async void OnCharacter2Tapped(object sender, EventArgs e)
    {
        // 判断是否已经是选中状态
        if (_selectedIndex == 2)
        {
            // 如果点的是已选中的，则取消选中
            _selectedIndex = 0;
            SelectionBorder2.IsVisible = false;
            StatsPanel.IsVisible = false;
            _selectedColor = Colors.Transparent;
        }
        else
        {
            // 否则，执行选中逻辑
            _selectedIndex = 2;
            SelectionBorder1.IsVisible = false;
            SelectionBorder2.IsVisible = true;
            StatsPanel.IsVisible = true;
            WarningLabel.IsVisible = false;

            _selectedHp = 70;
            _selectedAtk = 10;
            _selectedCd = 1.5;
            _selectedDodge = 0.10;
            _selectedColor = Colors.DarkGray;

            UpdateDisplay();
        }

        // 播放缩放反馈
        if (sender is View clickedShape)
        {
            await clickedShape.ScaleToAsync(1.1, 100, Easing.CubicOut);
            await clickedShape.ScaleToAsync(1.0, 100, Easing.CubicIn);
        }
    }

    private void UpdateDisplay()
    {
        HpLabel.Text = $"HP: {_selectedHp}";
        AtkLabel.Text = $"ATK: {_selectedAtk}";
        CdLabel.Text = $"CD: {_selectedCd:F1}s";
        DodgeLabel.Text = $"DODGE: {_selectedDodge * 100:F0}%";
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        if (_selectedIndex == 0)
        {
            WarningLabel.IsVisible = true;
            await WarningLabel.TranslateToAsync(-10, 0, 50);
            await WarningLabel.TranslateToAsync(10, 0, 50);
            await WarningLabel.TranslateToAsync(0, 0, 50);
            return;
        }

        await OkButton.ScaleToAsync(0.9, 100, Easing.CubicOut);
        await OkButton.ScaleToAsync(1.0, 100, Easing.CubicIn);

        
        await Navigation.PushAsync(new BattleFieldPage(_selectedColor, _selectedHp, _selectedAtk, _selectedCd, _selectedDodge, _selectedIndex));
    }
}