using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MusicPlayer
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void MainPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 1300) VisualStateManager.GoToState(this, "Mobile", false);
            else VisualStateManager.GoToState(this, "Desktop", false);
        }

        private void OpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            MusicControl.Engine.Select();
        }
    }
}