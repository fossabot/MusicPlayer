using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MusicPlayer.Shared.Engine;

namespace MusicPlayer.Shared.Pages
{
    public sealed partial class Player : Page
    {
        public Player()
        {
            this.InitializeComponent();
        }

        private AudioEngine Engine = new AudioEngine();

        private void Player_OnLoaded(object sender, RoutedEventArgs e)
        {
            Uno.Foundation.WebAssemblyRuntime.InvokeJS("document.getElementById('" + EngineFrame.HtmlId + "').innerHTML = '<canvas id=\"canvas\" style=\"position: fixed; left: 0; top: 0; width: 100%; height: 100%;\"><audio id=\"audio\" style=\"visibility:hidden;\" controls><input id=\"select\" type=\"file\" accept=\"audio/*,.radio\">';");

            var timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = new TimeSpan(0, 0, 0,0,100);
            timer.Start();
        }

        private string _fileName = "";
        private double _duration;

        private void TimerOnTick(object sender, object e)
        {
            PausePlaySymbol.Symbol = Engine.IsPlaying() ? Symbol.Pause : Symbol.Play;

            #region File

            var oldFile = _fileName;
            var newFile = Engine.GetFileName();

            if (!string.Equals(oldFile, newFile, StringComparison.CurrentCultureIgnoreCase))
            {
                _fileName = newFile;
                OnFileChanged();
            }

            #endregion

            #region Position

            var oldDuration = _duration;
            var newDuration = Engine.GetDuration();

            if (Math.Abs(oldDuration - newDuration) > 1)
            {
                _duration = newDuration;

                if (double.IsNaN(_duration) || double.IsInfinity(_duration))
                {
                    SongSeekBar.Maximum = 0;
                    SongDuration.Text = "00:00";
                }
                else
                {
                    SongSeekBar.Maximum = _duration;
                    SongDuration.Text = TimeSpan.FromSeconds(_duration).ToString(@"mm\:ss");
                }
            }

            if (double.IsNaN(_duration) || double.IsInfinity(_duration)) SongSeekBar.Value = 0;
            else SongSeekBar.Value = Engine.GetPosition();

            #endregion
        }

        private void OnFileChanged()
        {
            if (string.IsNullOrEmpty(_fileName)) return;

            NavigationContentToolsTitle.Text = Path.GetFileNameWithoutExtension(_fileName);

            Engine.Play();
        }

        private bool isPinChecked;
        private bool isRepeatChecked;
        private bool isMuteChecked;

        private void Shuffle_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;

            isPinChecked = !isPinChecked;
            btn.BorderBrush = isPinChecked ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Transparent);
        }

        private void Repeat_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;

            isRepeatChecked = !isRepeatChecked;
            btn.BorderBrush = isRepeatChecked ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Transparent);
        }

        private void Pin_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;

            isPinChecked = !isPinChecked;
            btn.BorderBrush = isPinChecked ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Transparent);
        }

        private void Mute_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;

            isMuteChecked = !isMuteChecked;
            btn.BorderBrush = isMuteChecked ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Transparent);

            if (!isMuteChecked) Engine.SetVolume(VolumeSeekBar.Value);
            else Engine.SetVolume(0);
        }

        private void PausePlay_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.PausePlay();
        }

        private void Volume_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Engine.SetVolume(((Slider) sender).Value);
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            Engine.Select();
        }

        private void SeekBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var oldTime = TimeSpan.FromSeconds(e.OldValue).TotalMilliseconds;
            var newTime = TimeSpan.FromSeconds(e.NewValue).TotalMilliseconds;
            var span = Math.Abs(newTime - oldTime);

            SongCurrentPosition.Text = TimeSpan.FromSeconds(e.NewValue).ToString(@"mm\:ss");

            if (span < 500) return;

            Engine.SetPosition(e.NewValue);
        }
    }
}
