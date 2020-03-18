using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Uno.Foundation;
using File = MusicPlayer.Shared.Helpers.File;

namespace MusicPlayer.Shared.Engine
{
    public class AudioEngine : INotifyPropertyChanged
    {
        private void TimerOnTick(object sender, object e)
        {
            #region isPlaying

            bool IsPlayingMethod()
            {
                var stream = File.GetStreamFromResource(@"isPlaying.js", GetType());
                var reader = new StreamReader(stream);
                var function = reader.ReadToEnd();

                return bool.Parse(InvokeJs(function));
            }

            IsPlaying = IsPlayingMethod();

            #endregion

            #region GetFileName

            string GetFileNameMethod()
            {
                var stream = File.GetStreamFromResource(@"getFile.js", GetType());
                var reader = new StreamReader(stream);
                var function = reader.ReadToEnd();

                return InvokeJs(function);
            }

            FileName = GetFileNameMethod();

            #endregion

            #region GetChannelLenght

            double GetChannelLengthMethod()
            {
                var stream = File.GetStreamFromResource(@"getDuration.js", GetType());
                var reader = new StreamReader(stream);
                var function = reader.ReadToEnd();

                var result = InvokeJs(function).Replace(".", ",");
                ;

                return result.Equals("Infinity", StringComparison.OrdinalIgnoreCase)
                    ? double.PositiveInfinity
                    : double.Parse(result);
            }

            ChannelLength = GetChannelLengthMethod();

            #endregion

            #region GetChannelPosition

            double GetChannelPositionMethod()
            {
                var stream = File.GetStreamFromResource(@"getPosition.js", GetType());
                var reader = new StreamReader(stream);
                var function = reader.ReadToEnd();

                var result = InvokeJs(function).Replace(".", ",");
                return result.Equals("Infinity", StringComparison.OrdinalIgnoreCase)
                    ? double.PositiveInfinity
                    : double.Parse(result);
            }

            if (double.IsNaN(ChannelLength) || double.IsInfinity(ChannelLength)) ChannelPosition = 0;
            else ChannelPosition = GetChannelPositionMethod();

            #endregion
        }

        private string InvokeJs(string jsCode)
        {
            return !_isLoaded ? null : WebAssemblyRuntime.InvokeJS(jsCode);
        }

        #region Functions

        public void Select()
        {
            if (!_isLoaded) return;

            var stream = File.GetStreamFromResource(@"Select.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            try
            {
                InvokeJs(function);
            }
            catch (Exception e)
            {
                //TODO: Remove Try-Catch
                Console.WriteLine(e);
            }
        }

        public void Play(string link = null)
        {
            if (!_isLoaded) return;

            InvokeJs("document.getElementById(\"audio\").pause();");

            var stream = File.GetStreamFromResource(@"PlaySong.js", GetType());
            var reader = new StreamReader(stream);
            var function = link == null
                ? reader.ReadToEnd()
                : reader.ReadToEnd().Replace("URL.createObjectURL(input.files[0])", "\'" + link + "\'");

            InvokeJs(function);
        }

        public void Stop()
        {
            if (!_isLoaded) return;

            var stream = File.GetStreamFromResource(@"Stop.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            InvokeJs(function);
        }

        public void PausePlay()
        {
            if (!_isLoaded) return;

            var stream = File.GetStreamFromResource(@"PausePlay.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            InvokeJs(function);
        }

        public void SetVolume(double volume, bool tmp = false)
        {
            if (!_isLoaded) return;

            var stream = File.GetStreamFromResource(@"setVolume.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd().Replace("$Volume", (volume / 100).ToString(CultureInfo.InvariantCulture));

            InvokeJs(function);

            if (!tmp) Volume = volume;
        }

        public void SetPosition(double time)
        {
            if (!_isLoaded) return;

            var stream = File.GetStreamFromResource(@"setPosition.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd().Replace("$Time", time.ToString(CultureInfo.InvariantCulture));

            InvokeJs(function);
        }

        public void Mute()
        {
            if (IsMuted)
            {
                SetVolume(Volume);
                IsMuted = false;
            }
            else
            {
                SetVolume(0, true);
                IsMuted = true;
            }
        }

        // TODO: Shuffle mode
        public void Shuffle()
        {
            if (IsShuffle)
                IsShuffle = false;
            else
                IsShuffle = true;

            throw new NotImplementedException();
        }

        // TODO: Play previous song
        public void PlayPrevious()
        {
            throw new NotImplementedException();
        }

        // TODO: Play next song
        public void PlayNext()
        {
            throw new NotImplementedException();
        }

        // TODO: Repeat all songs
        public void RepeatAll()
        {
            if (IsRepeatAll)
                IsRepeatAll = false;
            else
                IsRepeatAll = true;

            throw new NotImplementedException();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        // TODO: Add Playlist function

        #region Properties

        private bool _isPlaying;
        private bool _isMuted;
        private bool _isShuffle;
        private bool _isRepeatAll;
        private string _fileName = "";
        private double _channelLength;
        private double _channelPosition;
        private double _volume = 100;

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying == value) return;

                _isPlaying = value;
                OnPropertyChanged();
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            private set
            {
                if (_isMuted == value) return;

                _isMuted = value;
                OnPropertyChanged();
            }
        }

        public bool IsShuffle
        {
            get => _isShuffle;
            private set
            {
                if (_isShuffle == value) return;

                _isShuffle = value;
                OnPropertyChanged();
            }
        }

        public bool IsRepeatAll
        {
            get => _isRepeatAll;
            private set
            {
                if (_isRepeatAll == value) return;

                _isRepeatAll = value;
                OnPropertyChanged();
            }
        }

        public string FileName
        {
            get => _fileName;
            private set
            {
                if (_fileName == value || string.IsNullOrEmpty(value) ||
                    string.Equals(_fileName, value, StringComparison.CurrentCultureIgnoreCase)) return;

                _fileName = value;
                Play();
                OnPropertyChanged();
            }
        }

        public double ChannelLength
        {
            get => _channelLength;
            private set
            {
                if (_channelLength.Equals(value)) return;

                _channelLength = value;
                OnPropertyChanged();
            }
        }

        public double ChannelPosition
        {
            get => _channelPosition;
            private set
            {
                if (_channelPosition.Equals(value)) return;

                _channelPosition = value;
                OnPropertyChanged();
            }
        }

        public double Volume
        {
            get => _volume;
            private set
            {
                if (_volume.Equals(value)) return;

                _volume = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Load

        private bool _isLoaded;

        public void Load(UIElement element)
        {
            const string html =
                "<canvas id=\"canvas\" style=\"position: fixed; left: 0; top: 0; width: 100%; height: 100%;\"><audio id=\"audio\" preload=\"none\" style=\"visibility:hidden;\" controls autoplay></audio><input id=\"select\" type=\"file\" accept=\"audio/*,.radio\"></canvas>";

            WebAssemblyRuntime.InvokeJS("document.getElementById('" + element.HtmlId + "').innerHTML = '" + html +
                                        "';");

            var timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            _isLoaded = true;
        }

        #endregion
    }
}