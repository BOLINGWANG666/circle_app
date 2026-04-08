
using CommunityToolkit.Maui.Views;

namespace Circle.Pages;

public partial class ChooseCharacter : ContentPage
{
    private bool isSelected = false; 

    //  1. 定义类级别的“状态变量”，用来临时存储玩家选中的数据
    private int _selectedHp;
    private int _selectedAtk;
    private double _selectedCd;
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
            AudioPlayer.Source = MediaSource.FromResource("tapstart.mp3");
        }
    }

    private async void OnCharacterTapped(object sender, EventArgs e)
    {
        
        isSelected = !isSelected;

        if (isSelected)
        {
            SelectionBorder.IsVisible = true;
            StatsPanel.IsVisible = true;
            WarningLabel.IsVisible = false;

            //  2. 玩家点击这个角色时，立刻将其属性装载到状态变量中
            _selectedHp = 50;                             
            _selectedAtk = 15;
            _selectedCd = 1.2;
            _selectedColor = Colors.Gray;
            // (如果是法师，这里可能是 Hp=60, Atk=25, Color=Blue)

            await CharacterCircle.ScaleToAsync(1.1, 100, Easing.CubicOut);
            await CharacterCircle.ScaleToAsync(1.0, 100, Easing.CubicIn);
        }
        else
        {
            SelectionBorder.IsVisible = false;
            StatsPanel.IsVisible = false;
        }
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        if (!isSelected)
        {
            WarningLabel.IsVisible = true;
            await WarningLabel.TranslateToAsync(-10, 0, 50);
            await WarningLabel.TranslateToAsync(10, 0, 50);
            await WarningLabel.TranslateToAsync(0, 0, 50);
            
         
            return;
        }

        SoundManager.PlayClick();
        // --- 已选中，执行正常逻辑 ---
        await OkButton.ScaleToAsync(0.9, 100, Easing.CubicOut);
        await OkButton.ScaleToAsync(1.0, 100, Easing.CubicIn);
        
        


        // 3. 将变量中存储的最终数据传递给战场
        await Navigation.PushAsync(new BattleFieldPage(_selectedColor, _selectedHp, _selectedAtk, _selectedCd));
    }

    
}