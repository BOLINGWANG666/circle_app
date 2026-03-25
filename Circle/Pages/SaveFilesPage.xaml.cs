using Circle.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Circle.Pages;

public class SavePageGroup
{
    public PlayerSaveData? Slot1 { get; set; }
    public PlayerSaveData? Slot2 { get; set; }
    public PlayerSaveData? Slot3 { get; set; }

    // 新增独立的可视化序号，彻底屏蔽数据库的自增乱码
    public int Slot1DisplayId { get; set; }
    public int Slot2DisplayId { get; set; }
    public int Slot3DisplayId { get; set; }

    public bool IsSlot1Visible => Slot1 != null;
    public bool IsSlot2Visible => Slot2 != null;
    public bool IsSlot3Visible => Slot3 != null;
}

public partial class SaveFilesPage : ContentPage
{
    public ObservableCollection<SavePageGroup> PagedSaves { get; set; } = new ObservableCollection<SavePageGroup>();

    private PlayerSaveData? _selectedSave = null;
    private Border? _selectedBorderUI = null;
    private int _currentPageIndex = 0;

    public SaveFilesPage()
    {
        InitializeComponent();
        SavesCarousel.ItemsSource = PagedSaves;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadSavesData();
    }

    private void LoadSavesData()
    {
        var allSaves = Circle.ViewModels.CharacterViewModel.Current?.Characters?.OrderBy(x => x.Id).ToList() ?? new List<PlayerSaveData>();
        PagedSaves.Clear();

        int displayCounter = 1; // 永远从 1 开始按绝对顺序递增

        for (int i = 0; i < allSaves.Count; i += 3)
        {
            PagedSaves.Add(new SavePageGroup
            {
                Slot1 = i < allSaves.Count ? allSaves[i] : null,
                Slot2 = i + 1 < allSaves.Count ? allSaves[i + 1] : null,
                Slot3 = i + 2 < allSaves.Count ? allSaves[i + 2] : null,

                // 按实际排列顺序依次贴上连续的标签
                Slot1DisplayId = i < allSaves.Count ? displayCounter++ : 0,
                Slot2DisplayId = i + 1 < allSaves.Count ? displayCounter++ : 0,
                Slot3DisplayId = i + 2 < allSaves.Count ? displayCounter++ : 0,
            });
        }

        _selectedSave = null;
        _selectedBorderUI = null;
        WarningLabel.IsVisible = false;

        _currentPageIndex = 0;
    }

    private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
    {
        _currentPageIndex = e.CurrentPosition;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        await Navigation.PopAsync();
    }

    private async void OnPrevSaveClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        if (PagedSaves.Count > 1)
        {
            _currentPageIndex = _currentPageIndex > 0 ? _currentPageIndex - 1 : PagedSaves.Count - 1;
            SavesCarousel.ScrollTo(_currentPageIndex, position: ScrollToPosition.Center, animate: true);
            await Task.Delay(300);
        }

        button.IsEnabled = true;
    }

    private async void OnNextSaveClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        if (PagedSaves.Count > 1)
        {
            _currentPageIndex = _currentPageIndex < PagedSaves.Count - 1 ? _currentPageIndex + 1 : 0;
            SavesCarousel.ScrollTo(_currentPageIndex, position: ScrollToPosition.Center, animate: true);
            await Task.Delay(300);
        }

        button.IsEnabled = true;
    }

    private async void OnSaveSlotTapped(object sender, TappedEventArgs e)
    {
        SoundManager.PlayClick();
        WarningLabel.IsVisible = false;

        if (sender is Border cardBorder)
        {
            if (_selectedBorderUI == cardBorder) return;

            if (_selectedBorderUI != null)
            {
                _selectedBorderUI.CancelAnimations();
                _selectedBorderUI.BackgroundColor = Colors.White;
                _ = _selectedBorderUI.ScaleToAsync(1.0, 100, Easing.CubicOut);
            }

            _selectedBorderUI = cardBorder;

            cardBorder.CancelAnimations();
            cardBorder.BackgroundColor = Color.Parse("#E0E0E0");
            await cardBorder.ScaleToAsync(0.95, 100, Easing.CubicOut);
        }

        if (e.Parameter is PlayerSaveData tappedSave)
        {
            _selectedSave = tappedSave;
        }
    }

    private async void OnLoadClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        if (_selectedSave == null)
        {
            WarningLabel.Text = "Please select a file to load";
            WarningLabel.IsVisible = true;
            await WarningLabel.TranslateToAsync(-10, 0, 50);
            await WarningLabel.TranslateToAsync(10, 0, 50);
            await WarningLabel.TranslateToAsync(0, 0, 50);

            button.IsEnabled = true;
            return;
        }

        
        await Navigation.PushAsync(new BattleFieldPage(_selectedSave, Colors.Grey));

        button.IsEnabled = true;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        if (_selectedSave == null)
        {
            WarningLabel.Text = "Please select a file to delete";
            WarningLabel.IsVisible = true;
            await WarningLabel.TranslateToAsync(-10, 0, 50);
            await WarningLabel.TranslateToAsync(10, 0, 50);
            await WarningLabel.TranslateToAsync(0, 0, 50);

            button.IsEnabled = true;
            return;
        }

        var allSaves = Circle.ViewModels.CharacterViewModel.Current?.Characters?.OrderBy(x => x.Id).ToList() ?? new List<PlayerSaveData>();
        int visualId = allSaves.FindIndex(x => x.Id == _selectedSave.Id) + 1;

        bool confirm = await DisplayAlertAsync("Delete Save", $"Are you sure you want to delete Save ID: {visualId}?", "Yes", "No");
        if (!confirm)
        {
            button.IsEnabled = true;
            return;
        }

        // 直接将选中的存档物理删除，因为有了独立显示编号，根本不需要再覆盖移动数据了
        Circle.ViewModels.CharacterViewModel.Current?.DeleteCharacter(_selectedSave);

        LoadSavesData();
        if (PagedSaves.Count > 0)
        {
            SavesCarousel.ScrollTo(0, position: ScrollToPosition.Center, animate: false);
        }

        button.IsEnabled = true;
    }
}