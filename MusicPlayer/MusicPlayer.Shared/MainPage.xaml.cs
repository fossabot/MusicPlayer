using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Uno.Foundation;

namespace MusicPlayer
{
    public partial class MainPage : Page
    {
        public static readonly string UserAgentPostfix = "[RH Music PWA Wrapper]";

        public MainPage()
        {
            InitializeComponent();
        }

        public static bool isPwaWrapper { get; private set; }
        public static string UserAgent { get; private set; }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            UserAgent = WebAssemblyRuntime.InvokeJS("navigator.userAgent;");
            isPwaWrapper = UserAgent.Contains(UserAgentPostfix);


            if (!isPwaWrapper) return;

            TitleBar.Visibility = Visibility.Visible;
            WebAssemblyRuntime.InvokeJS($"document.getElementById('{WindowDragRegion.HtmlId}')" +
                                        ".style = '-webkit-app-region: drag';");
            WebAssemblyRuntime.InvokeJS($"document.getElementById('{WindowMinimizeButton.HtmlId}')" +
                                        ".addEventListener('click', () => window.ipcRenderer.send('app:minimize'));");
            WebAssemblyRuntime.InvokeJS($"document.getElementById('{WindowMinMaxButton.HtmlId}')" +
                                        ".addEventListener('click', () => window.ipcRenderer.send('app:min-max'));");
            WebAssemblyRuntime.InvokeJS($"document.getElementById('{WindowCloseButton.HtmlId}')" +
                                        ".addEventListener('click', () => window.ipcRenderer.send('app:quit'));");
        }

        private void MainPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 800) VisualStateManager.GoToState(this, "Phone", false);
            else if (e.NewSize.Width < 1300) VisualStateManager.GoToState(this, "Mobile", false);
            else VisualStateManager.GoToState(this, "Desktop", false);
        }

        private async void Play_OnClick(object sender, RoutedEventArgs e)
        {
            MusicControl.Engine.AddSongs = false;

            if (Uri.TryCreate(Url.Text, UriKind.Absolute, out var uriResult))
            {
                var song = await MusicControl.Engine.CreateSongAsync(uriResult.ToString());

                await MusicControl.Engine.Play(song);

                Url.Text = "";
            }
            else
            {
                MusicControl.Engine.OpenFileDialog();
            }
        }

        private async void AddToPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            MusicControl.Engine.AddSongs = true;

            if (Uri.TryCreate(Url.Text, UriKind.Absolute, out var uriResult))
            {
                var song = await MusicControl.Engine.CreateSongAsync(uriResult.ToString());

                var playlist = MusicControl.Engine.Playlist;
                playlist.Add(song);
                MusicControl.Engine.Playlist = playlist;

                if (MusicControl.Engine.CurrentPlayBack == null) await MusicControl.Engine.Play(song);

                Url.Text = "";
            }
            else
            {
                MusicControl.Engine.OpenFileDialog();
            }
        }

        private void ClearPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            var playlist = MusicControl.Engine.Playlist;
            playlist.Clear();
            MusicControl.Engine.Playlist = playlist;
        }

        private async void StartPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            if (MusicControl.Engine.Playlist.Count < 1) return;

            await MusicControl.Engine.Play(MusicControl.Engine.Playlist[0]);
        }

        private void WindowButtons_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var border = (Border) sender;
            var bc = new BrushConverter();

            switch (border.Name)
            {
                case "WindowMinimizeButton":
                    border.Background = (Brush) bc.ConvertFrom("#1B5E20");
                    break;
                case "WindowMinMaxButton":
                    border.Background = (Brush) bc.ConvertFrom("#BF360C");
                    break;
                case "WindowCloseButton":
                    border.Background = (Brush) bc.ConvertFrom("#B71C1C");
                    break;
            }
        }

        private void WindowButtons_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var border = (Border) sender;
            var bc = new BrushConverter();

            switch (border.Name)
            {
                case "WindowMinimizeButton":
                    border.Background = (Brush) bc.ConvertFrom("#2E7D32");
                    break;
                case "WindowMinMaxButton":
                    border.Background = (Brush) bc.ConvertFrom("#D84315");
                    break;
                case "WindowCloseButton":
                    border.Background = (Brush) bc.ConvertFrom("#C62828");
                    break;
            }
        }

        private void WindowButtons_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var border = (Border) sender;
            var bc = new BrushConverter();

            switch (border.Name)
            {
                case "WindowMinimizeButton":
                    border.Background = (Brush) bc.ConvertFrom("#33691E");
                    break;
                case "WindowMinMaxButton":
                    border.Background = (Brush) bc.ConvertFrom("#DD2C00");
                    break;
                case "WindowCloseButton":
                    border.Background = (Brush) bc.ConvertFrom("#D50000");
                    break;
            }
        }
    }
}