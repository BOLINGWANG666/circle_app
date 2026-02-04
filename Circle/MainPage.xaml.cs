namespace Circle
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        int count1 = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void OnCounterClicked1(object? sender, EventArgs e)
        {
            count1++;

            if (count1 == 1)
                CounterBtn1.Text = $"Clicked {count1} time";
            else
                CounterBtn1.Text = $"Clicked {count1} times";

            SemanticScreenReader.Announce(CounterBtn1.Text);
        }
    }
}
