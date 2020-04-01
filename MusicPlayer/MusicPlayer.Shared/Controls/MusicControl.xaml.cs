using System;
using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using MusicPlayer.Shared.Engine;

namespace MusicPlayer.Shared.Controls
{
    public sealed partial class MusicControl : UserControl
    {
        public MusicControl()
        {
            InitializeComponent();
        }

        public AudioEngine Engine { get; } = new AudioEngine();

        private void MusicControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 800) VisualStateManager.GoToState(this, "Phone", false);
            else if (e.NewSize.Width < 1300) VisualStateManager.GoToState(this, "Tablet", false);
            else VisualStateManager.GoToState(this, "Desktop", false);
        }

        private void MusicControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            Engine.Load(EngineFrame);

            Engine.PropertyChanged += AudioEngine_PropertyChanged;
        }

        private void AudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentPlayBack":
                    SongTitle.Text = Engine.CurrentPlayBack == null
                        ? "RH Music Player"
                        : Engine.CurrentPlayBack.Title;
                    break;
                case "IsPlaying":
                    PlaySymbol.Visibility = Engine.IsPlaying ? Visibility.Collapsed : Visibility.Visible;
                    PauseSymbol.Visibility = Engine.IsPlaying ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "IsMuted":
                    MuteToggleButton.BorderBrush = Engine.IsMuted
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Colors.Transparent);
                    break;
                case "Shuffle":
                    ShuffleToggleButton.BorderBrush = Engine.Shuffle
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Colors.Transparent);
                    break;
                case "RepeatAll":
                    RepeatAllToggleButton.BorderBrush = Engine.RepeatAll
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Colors.Transparent);
                    break;
                case "CurrentChannelLength":
                    var isEnabled = Engine.CurrentPlayBack != null &&
                                    Engine.CurrentPlayBack.Provider != AudioEngine.SongProvider.LiveStream &&
                                    !double.IsNaN(Engine.CurrentChannelLength);

                    SongSeekBar.IsEnabled = isEnabled;
                    SongSeekBar.Maximum = isEnabled ? Engine.CurrentChannelLength : 0;
                    SongDuration.Text = isEnabled
                        ? TimeSpan.FromSeconds(Engine.CurrentChannelLength).ToString(@"mm\:ss")
                        : "00:00";
                    break;
                case "ChannelPosition":
                    SongSeekBar.Value = Engine.ChannelPosition;
                    break;
                case "Volume":
                    VolumeSeekBar.Value = Engine.Volume;
                    break;
            }
        }

        private void SeekBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var oldTime = TimeSpan.FromSeconds(e.OldValue).TotalMilliseconds;
            var newTime = TimeSpan.FromSeconds(e.NewValue).TotalMilliseconds;
            var span = Math.Abs(newTime - oldTime);

            SongPosition.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"mm\:ss");

            if (span < 500) return;

            Engine.ChannelPosition = e.NewValue;
        }

        private void Shuffle_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.Shuffle = !Engine.Shuffle;
        }

        private async void Previous_OnClick(object sender, RoutedEventArgs e)
        {
            await Engine.PlayPrevious();
        }

        private async void PausePlay_OnClick(object sender, RoutedEventArgs e)
        {
            switch (Engine.CurrentPlayBack)
            {
                case null when Engine.Playlist.Count == 0:
                    Engine.OpenFileDialog();
                    break;
                case null:
                    await Engine.Play(Engine.Playlist[0]);
                    break;
                default:
                    Engine.PausePlay();
                    break;
            }
        }

        private async void Next_OnClick(object sender, RoutedEventArgs e)
        {
            await Engine.PlayNext();
        }

        private void RepeatAll_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.RepeatAll = !Engine.RepeatAll;
        }

        // TODO: Pin song function
        private void Pin_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Volume_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue > Convert.ToDouble(100 / 3 * 2))
            {
                VolumeSeekBarHigh.Visibility = Visibility.Visible;
                VolumeSeekBarMedium.Visibility = Visibility.Collapsed;
                VolumeSeekBarLow.Visibility = Visibility.Collapsed;
                VolumeSeekBarMute.Visibility = Visibility.Collapsed;
            }
            else if (e.NewValue > Convert.ToDouble(100 / 3))
            {
                VolumeSeekBarHigh.Visibility = Visibility.Collapsed;
                VolumeSeekBarMedium.Visibility = Visibility.Visible;
                VolumeSeekBarLow.Visibility = Visibility.Collapsed;
                VolumeSeekBarMute.Visibility = Visibility.Collapsed;
            }
            else if (e.NewValue > 0)
            {
                VolumeSeekBarHigh.Visibility = Visibility.Collapsed;
                VolumeSeekBarMedium.Visibility = Visibility.Collapsed;
                VolumeSeekBarLow.Visibility = Visibility.Visible;
                VolumeSeekBarMute.Visibility = Visibility.Collapsed;
            }
            else if (e.NewValue == 0)
            {
                VolumeSeekBarHigh.Visibility = Visibility.Collapsed;
                VolumeSeekBarMedium.Visibility = Visibility.Collapsed;
                VolumeSeekBarLow.Visibility = Visibility.Collapsed;
                VolumeSeekBarMute.Visibility = Visibility.Visible;
            }

            Engine.Volume = e.NewValue;
        }

        private void Mute_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.IsMuted = !Engine.IsMuted;
        }
    }
}