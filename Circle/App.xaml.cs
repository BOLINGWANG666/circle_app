namespace Circle
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // 1. 创建一个包裹着 MainPage 的 NavigationPage
            // 可以使用 Navigation.PushAsync 跳转
            var navPage = new NavigationPage(new MainPage());

            // 2. 将这个 NavigationPage 放入一个新的 Window 中
            var window = new Window(navPage);

            //  Windows 测试的窗口大小限制
            // 设定最小尺寸
            window.MinimumWidth = 800;
            window.MinimumHeight = 600;

            

            // 4. 返回设置好的窗口
            return window;
        }
    }
}