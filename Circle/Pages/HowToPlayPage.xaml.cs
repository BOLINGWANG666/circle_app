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

        // Scroll back to the top when switching pages
        ContentScrollView.ScrollToAsync(0, 0, false);

        // Use Dispatcher to ensure MAUI has fully rendered the new page's elements before measuring height to prevent errors!
        Dispatcher.Dispatch(() =>
        {
            CheckScrollHintVisibility();
        });

        if (animate)
        {
            _ = ContentContainer.FadeToAsync(1, 200, Easing.Linear);
        }
    }

    // Arrow logic

    // Independent height check method
    private void CheckScrollHintVisibility()
    {
        if (ScrollDownHintLayer == null || ContentScrollView == null) return;

        // Only when the actual total content height > the container's physical height, it means content is hidden and a scroll down hint is needed
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

    // Also check once when the container size changes
    private void OnScrollViewSizeChanged(object sender, EventArgs e)
    {
        CheckScrollHintVisibility();
    }

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        if (ScrollDownHintLayer != null)
        {
            // If scrolled down more than 5 pixels, hide immediately
            if (e.ScrollY > 5 && ScrollDownHintLayer.IsVisible)
            {
                ScrollDownHintLayer.IsVisible = false;
                _ = ScrollDownHintLayer.FadeToAsync(0, 200, Easing.Linear);
            }
            // If the player scrolls back to the top and the content is indeed longer than the screen (needs scrolling), show the arrow again
            else if (e.ScrollY <= 5 && !ScrollDownHintLayer.IsVisible && ContentScrollView.ContentSize.Height > ContentScrollView.Height)
            {
                ScrollDownHintLayer.IsVisible = true;
                _ = ScrollDownHintLayer.FadeToAsync(0.5, 200, Easing.Linear);
            }
        }
    }
}