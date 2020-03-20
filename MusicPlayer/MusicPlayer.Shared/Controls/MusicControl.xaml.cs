using System;
using System.ComponentModel;
using System.IO;
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
                case "IsPlaying":
                    if (Engine.IsPlaying)
                    {
                        PlaySymbol.Visibility = Visibility.Collapsed;
                        PauseSymbol.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PlaySymbol.Visibility = Visibility.Visible;
                        PauseSymbol.Visibility = Visibility.Collapsed;
                    }

                    break;
                case "IsMuted":
                    MuteToggleButton.BorderBrush = Engine.IsMuted
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Colors.Transparent);
                    break;
                case "IsShuffle":
                    ShuffleToggleButton.BorderBrush = Engine.IsShuffle
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Colors.Transparent);
                    break;
                case "IsRepeatAll":
                    RepeatAllToggleButton.BorderBrush = Engine.IsRepeatAll
                        ? new SolidColorBrush(Colors.White)
                        : new SolidColorBrush(Colors.Transparent);
                    break;
                case "Title":
                    SongTitle.Text = Path.GetFileNameWithoutExtension(Engine.Title);
                    break;
                case "ChannelLength":
                    if (double.IsNaN(Engine.ChannelLength) || double.IsInfinity(Engine.ChannelLength))
                    {
                        SongSeekBar.IsEnabled = false;
                        SongSeekBar.Maximum = 0;
                        SongDuration.Text = "00:00";
                    }
                    else
                    {
                        SongSeekBar.IsEnabled = true;
                        SongSeekBar.Maximum = Engine.ChannelLength;
                        SongDuration.Text = TimeSpan.FromSeconds(Engine.ChannelLength).ToString(@"mm\:ss");
                    }

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

            Engine.SetPosition(e.NewValue);
        }

        private void Shuffle_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.Shuffle();
        }

        private void Previous_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.PlayPrevious();
        }

        private void PausePlay_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.PausePlay();
        }

        private void Next_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.PlayNext();
        }

        private void RepeatAll_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.RepeatAll();
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

            Engine.SetVolume(e.NewValue);
        }

        private void Mute_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.Mute();
        }
    }
}