namespace Circle.Pages;

public partial class HowToPlayPage : ContentPage
{
    private int _currentPageIndex = 1;
    private readonly int _totalPages = 5;

    public HowToPlayPage()
    {
        InitializeComponent();
        UpdatePageDisplay(false);
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

    private async void OnPrevClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        if (_currentPageIndex > 1)
        {
            _currentPageIndex--;
            UpdatePageDisplay(true);
        }

        button.IsEnabled = true;
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;

        SoundManager.PlayClick();
        await button.ScaleToAsync(0.95, 100, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 100, Easing.CubicIn);

        if (_currentPageIndex < _totalPages)
        {
            _currentPageIndex++;
            UpdatePageDisplay(true);
        }

        button.IsEnabled = true;
    }

    private void UpdatePageDisplay(bool animate)
    {
        PageIndicator.Text = $"{_currentPageIndex} / {_totalPages}";

        if (animate)
        {
            ContentContainer.Opacity = 0;
        }

        Page1.IsVisible = (_currentPageIndex == 1);
        Page2.IsVisible = (_currentPageIndex == 2);
        Page3.IsVisible = (_currentPageIndex == 3);
        Page4.IsVisible = (_currentPageIndex == 4);
        Page5.IsVisible = (_currentPageIndex == 5);

        // 切换页面时，滚回顶端
        ContentScrollView.ScrollToAsync(0, 0, false);

        // 使用 Dispatcher 确保 MAUI 已经把新一页的元素全部渲染完，再去测量高度，杜绝误差！
        Dispatcher.Dispatch(() =>
        {
            CheckScrollHintVisibility();
        });

        if (animate)
        {
            _ = ContentContainer.FadeToAsync(1, 200, Easing.Linear);
        }
    }

    // 箭头逻辑

    // 独立的高度检查方法
    private void CheckScrollHintVisibility()
    {
        if (ScrollDownHintLayer == null || ContentScrollView == null) return;

        //  只有当内容实际总高度 > 容器物理高度，才说明有内容被挡住了，需要提示下滑
        if (ContentScrollView.ContentSize.Height > ContentScrollView.Height && ContentScrollView.Height > 0)
        {
            ScrollDownHintLayer.IsVisible = true;
            ScrollDownHintLayer.Opacity = 0.5;
        }
        else
        {
            ScrollDownHintLayer.IsVisible = false;
        }
    }

    // 容器大小发生改变时也检查一次
    private void OnScrollViewSizeChanged(object sender, EventArgs e)
    {
        CheckScrollHintVisibility();
    }

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (ScrollDownHintLayer != null)
        {
            // 如果往下划了超过 5 像素，立马隐身
            if (e.ScrollY > 5 && ScrollDownHintLayer.IsVisible)
            {
                ScrollDownHintLayer.IsVisible = false;
                _ = ScrollDownHintLayer.FadeToAsync(0, 200, Easing.Linear);
            }
            // 如果玩家滑回了顶端，且内容确实比屏幕长（需要下拉），才重新显示箭头
            else if (e.ScrollY <= 5 && !ScrollDownHintLayer.IsVisible && ContentScrollView.ContentSize.Height > ContentScrollView.Height)
            {
                ScrollDownHintLayer.IsVisible = true;
                _ = ScrollDownHintLayer.FadeToAsync(0.5, 200, Easing.Linear);
            }
        }
    }
}