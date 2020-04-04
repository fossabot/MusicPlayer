using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Uno.Extensions;
using Uno.Foundation;
using Uno.UI.Wasm;
using File = MusicPlayer.Shared.Helpers.File;

namespace MusicPlayer.Shared.Engine
{
    public class AudioEngine : INotifyPropertyChanged
    {
        public enum SongProvider
        {
            Unknown,
            File,
            YouTube,
            Stream,
            LiveStream
        }

        //public readonly string[] PlaylistExt = {".m3u", ".vlc", ".m3u8", ".xspf", ".b4s", ".jspf"};
        public readonly string[] SongExt = {".flac", ".m4a", ".mp3", ".ogg", ".opus", ".webm", ".wav"};
        //public readonly string[] UniversalExt = {".music", ".radio"};

        public async Task<Song> CreateSongAsync(string url, string fileName = null, string title = null)
        {
            if (!_isLoaded) return null;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var sUri))
            {
                //TODO: Link is not valid
                return null;
            }

            if (sUri.Scheme == Uri.UriSchemeFile) fileName = Path.GetFileName(sUri.ToString());

            if (sUri.Scheme == Uri.UriSchemeHttp || sUri.Scheme == Uri.UriSchemeHttps)
            {
                string[] ytHosts = {"www.youtube.com", "youtu.be"};

                if (ytHosts.Contains(sUri.Host, StringComparer.OrdinalIgnoreCase))
                {
                    async Task<string> GetTitleAsync()
                    {
                        if (!string.IsNullOrEmpty(title)) return title;

                        var httpClient = new HttpClient(new WasmHttpHandler());

                        var json = await httpClient.GetStringAsync(
                            "https://stream.api.rh-utensils.hampoelz.net/getInfos.php?url=" + sUri);

                        if (json.Equals("Not Found"))
                        {
                            //TODO: YouTube Video not found
                            return null;
                        }
                        else if (string.IsNullOrEmpty(json))
                        {
                            //TODO: YouTube Video fetch error
                            return null;
                        }

                        var id = new Random().Next().ToString();

                        WebAssemblyRuntime.InvokeJS("var tmpTitle = document.createElement('p'); tmpTitle.id = 'tmpTitle-" + id + "'; document.getElementById('canvas').appendChild(tmpTitle);");

                        RunFunction("getTitle", new[] { ("$Id", id), ("$Type", "youtube"), ("$Json", Base64Encode(json))});

                        var timeout = new Stopwatch();
                        timeout.Start();

                        var tmpTitle = "";

                        while (string.IsNullOrEmpty(tmpTitle))
                        {
                            await Task.Delay(100);

                            tmpTitle = Uri.UnescapeDataString(Base64Decode(
                                WebAssemblyRuntime.InvokeJS("document.getElementById('tmpTitle-" + id + "').innerHTML;")));

                            if (timeout.ElapsedMilliseconds <= 5000) continue;

                            tmpTitle = "Unknown YouTube Video";
                            break;
                        }

                        WebAssemblyRuntime.InvokeJS("document.getElementById('tmpTitle-" + id + "').remove();");
                        return tmpTitle.Equals("null") ? "Unknown YouTube Video" : tmpTitle;
                    }

                    var sTitle = await GetTitleAsync();

                    return sTitle == null ? null : new Song {Title = sTitle, Uri = sUri, Provider = SongProvider.YouTube};
                }
            }
            else if (fileName != null &&
                     SongExt.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
            {
                async Task<string> GetTitle()
                {
                    if (!string.IsNullOrEmpty(title)) return title;

                    var id = new Random().Next().ToString();

                    WebAssemblyRuntime.InvokeJS("var tmpTitle = document.createElement('p'); tmpTitle.id = 'tmpTitle-" + id + "'; document.getElementById('canvas').appendChild(tmpTitle);");

                    RunFunction("getTitle",
                        new[]
                        {
                            ("$Id", id),
                            ("$Type", "file"),
                            ("$isLocalFile", (sUri.Scheme == Uri.UriSchemeFile).ToString().ToLower()),
                            ("$Url", Base64Encode(Uri.EscapeDataString(sUri.ToString()))),
                            ("$FileName", Base64Encode(Uri.EscapeDataString(fileName)))
                        });

                    var timeout = new Stopwatch();
                    timeout.Start();

                    var tmpTitle = "";

                    while (string.IsNullOrEmpty(tmpTitle))
                    {
                        await Task.Delay(100);

                        tmpTitle = Uri.UnescapeDataString(
                            Base64Decode(
                                WebAssemblyRuntime.InvokeJS("document.getElementById('tmpTitle-" + id + "').innerHTML;")));

                        if (timeout.ElapsedMilliseconds <= 5000) continue;

                        tmpTitle = Path.GetFileNameWithoutExtension(fileName);
                        break;
                    }

                    WebAssemblyRuntime.InvokeJS("document.getElementById('tmpTitle-" + id + "').remove();");
                    return tmpTitle.Equals("null")
                        ? Path.GetFileNameWithoutExtension(fileName)
                        : tmpTitle;
                }

                return new Song {Title = await GetTitle(), Uri = sUri, Provider = SongProvider.File};
            }


            //TODO: Fetch error
            return null;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.Default.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.Default.GetString(base64EncodedBytes);
        }

        private string RunFunction(string name, IEnumerable<(string parameter, string value)> replace = null)
        {
            var stream = File.GetStreamFromResource($"{name}.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            if (replace == null) return InvokeJs(function);

            foreach (var (parameter, value) in replace) function = function.Replace(parameter, value);

            return InvokeJs(function);
        }

        private string InvokeJs(string jsCode)
        {
            return !_isLoaded ? null : WebAssemblyRuntime.InvokeJS(jsCode);
        }

        public class Song
        {
            public string Title { get; set; }

            public Uri Uri { get; set; }

            public SongProvider Provider { get; set; } = SongProvider.Unknown;
        }

        #region Events

        private async void OnHandleFile(object sender, HtmlCustomEventArgs e)
        {
            var song = await CreateSongAsync(e.Detail);

            var playlist = Playlist;
            playlist.Add(song);
            Playlist = playlist;

            if (CurrentPlayBack == null) await Play(song);
        }

        private async void TimerOnTick(object sender, object e)
        {
            if (!_isLoaded) return;

            IsPlaying = bool.Parse(RunFunction("isPlaying"));

            CurrentChannelLength = double.Parse(RunFunction("getDuration").Replace(".", ","));
            ChannelPosition = double.Parse(RunFunction("getPosition").Replace(".", ","));

            if (!IsPlaying && Math.Abs(ChannelPosition - CurrentChannelLength) < 0.5)
            {
                IsEnded = true;
                ChannelPosition = 0;

                if (PlaylistIndex == -1 && RepeatAll) PausePlay();
                else if (PlaylistIndex != -1 && PlaylistIndex != Playlist.Count - 1 || RepeatAll) await PlayNext();
            }
            else
            {
                IsEnded = false;
            }

            var data = RunFunction("getFiles", new[] {("$isPwaWrapper", MainPage.isPwaWrapper.ToString().ToLower())});

            if (string.IsNullOrEmpty(data)) return;

            var files = data.Split('|')[0].Split(',');
            var urls = data.Split('|')[1].Split(',');

            List<(string url, string fileName)> selectedFiles =
                files.Select((t, file) => (Uri.UnescapeDataString(Base64Decode(urls[file])),
                    Uri.UnescapeDataString(Base64Decode(t)))).ToList();

            foreach (var (url, fileName) in selectedFiles)
            {
                var selectedSong = await CreateSongAsync(url, fileName);
                var fistItem = selectedFiles[0] == (url, fileName);

                if (AddSongs && Playlist.All(song => song.Uri != selectedSong.Uri) &&
                    selectedSong.Provider != SongProvider.LiveStream)
                {
                    var playlist = Playlist;

                    playlist.Add(selectedSong);
                    Playlist = playlist;

                    if (fistItem && CurrentPlayBack == null) await Play(selectedSong);
                }
                else if (fistItem)
                {
                    await Play(selectedSong);
                }
            }
        }

        #endregion

        #region Functions

        public void OpenFileDialog()
        {
            if (!_isLoaded) return;

            RunFunction("_FileSelector", new[] {("$Multiselect", AddSongs.ToString().ToLower())});
        }

        public async Task Play(Song song)
        {
            if (!_isLoaded) return;

            if (song == null)
            {
                //TODO: Can't play Song
                return;
            }

            var audioUrl = "";

            switch (song.Provider)
            {
                case SongProvider.Unknown:
                    break;
                case SongProvider.File:
                    audioUrl = song.Uri.ToString();
                    break;
                case SongProvider.YouTube:
                    var httpClient = new HttpClient(new WasmHttpHandler());

                    var json = await httpClient.GetStringAsync(
                        "https://stream.api.rh-utensils.hampoelz.net/getLinks.php?url=" + song.Uri);
                    json = Regex.Replace(json, @"\t|\n|\r|    ", string.Empty);

                    var webmUrl = Base64Decode(RunFunction("getWebmUrl", new[] {("$Json", Base64Encode(json))}));

                    if (string.IsNullOrEmpty(webmUrl))
                    {
                        //TODO: Not supported YouTube Video
                        return;
                    }

                    audioUrl = "https://stream.api.rh-utensils.hampoelz.net/stream.php?url=" + webmUrl;
                    break;
                case SongProvider.Stream:
                case SongProvider.LiveStream:
                    audioUrl = Uri.UnescapeDataString(song.Uri.ToString());
                    audioUrl = "https://stream.api.rh-utensils.hampoelz.net/stream.php?url=" + audioUrl;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RunFunction("_SongPlay",
                new[]
                {
                    ("$isLocalFile", (song.Uri.Scheme == Uri.UriSchemeFile).ToString().ToLower()),
                    ("$Url", Base64Encode(Uri.EscapeDataString(audioUrl)))
                });

            CurrentPlayBack = song;
        }

        public void Stop()
        {
            if (!_isLoaded) return;

            RunFunction("_SongStop");

            CurrentPlayBack = null;
        }

        public void PausePlay()
        {
            if (!_isLoaded) return;

            RunFunction("_SongPausePlay");
        }

        #region PlaylistFunctions

        public async Task PlayPrevious()
        {
            if (!_isLoaded && Playlist.Count == 0) return;

            var lastIndex = Playlist.Count - 1;

            switch (PlaylistIndex)
            {
                case -1:
                    await Play(Playlist[0]);
                    break;
                case 0:
                    await Play(Playlist[lastIndex]);
                    break;
                default:
                    await Play(Playlist[PlaylistIndex - 1]);
                    break;
            }
        }

        public async Task PlayNext()
        {
            if (!_isLoaded && Playlist.Count == 0) return;

            var lastIndex = Playlist.Count - 1;

            if (PlaylistIndex == -1 || PlaylistIndex == lastIndex)
                await Play(Playlist[0]);
            else
                await Play(Playlist[PlaylistIndex + 1]);
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion


        #region Properties

        private bool _isPlaying;
        private bool _isEnded;
        private bool _isMuted;

        private bool _shuffle;
        private bool _repeatAll;

        private Song _playback;
        private List<Song> _playlist = new List<Song>();

        private double _channelLength;
        private double _channelPosition;
        private double _volume = 100;

        public bool AddSongs { get; set; } = true;

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying.Equals(value) || !_isLoaded) return;

                _isPlaying = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnded
        {
            get => _isEnded;
            private set
            {
                if (_isEnded.Equals(value) || !_isLoaded) return;

                _isEnded = value;
                OnPropertyChanged();
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted.Equals(value) || !_isLoaded) return;

                _isMuted = value;

                Volume = value ? 0 : Volume;

                OnPropertyChanged();
            }
        }

        public bool Shuffle
        {
            get => _shuffle;
            set
            {
                if (_shuffle.Equals(value)) return;

                // TODO: Shuffle mode

                _shuffle = value;
                OnPropertyChanged();
            }
        }

        public bool RepeatAll
        {
            get => _repeatAll;
            set
            {
                if (_repeatAll.Equals(value) || !_isLoaded) return;

                _repeatAll = value;
                OnPropertyChanged();
            }
        }

        public int PlaylistIndex => Playlist.FindIndex(song => song == CurrentPlayBack);

        public List<Song> Playlist
        {
            get => _playlist;
            set
            {
                if (_playlist == value || !_isLoaded) return;

                _playlist = value.Where(song =>
                    !string.IsNullOrEmpty(song.Title) && !string.IsNullOrEmpty(song.Uri.ToString()) &&
                    song.Provider != SongProvider.Unknown && song.Provider != SongProvider.LiveStream).ToList();

                OnPropertyChanged();
            }
        }

        public Song CurrentPlayBack
        {
            get => _playback;
            private set
            {
                if (_playback == value || !_isLoaded) return;

                _playback = value;
                OnPropertyChanged();
            }
        }

        public double CurrentChannelLength
        {
            get => _channelLength;
            private set
            {
                if (_channelLength.Equals(value) || !_isLoaded) return;

                _channelLength = value;
                OnPropertyChanged();
            }
        }

        public double ChannelPosition
        {
            get => _channelPosition;
            set
            {
                if (_channelPosition.Equals(value) || !_isLoaded) return;

                var oldTime = TimeSpan.FromSeconds(_channelPosition).TotalMilliseconds;
                var newTime = TimeSpan.FromSeconds(value).TotalMilliseconds;
                var span = Math.Abs(newTime - oldTime);

                if (span > 500)
                    RunFunction("setPosition", new[] {("$Time", value.ToString(CultureInfo.InvariantCulture))});

                _channelPosition = value;
                OnPropertyChanged();
            }
        }

        public double Volume
        {
            get => _volume;
            set
            {
                if (!_isLoaded) return;

                RunFunction("setVolume", new[] {("$Volume", (value / 100).ToString(CultureInfo.InvariantCulture))});

                if (!IsMuted) _volume = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Load

        private bool _isLoaded;

        public void Load(UIElement element)
        {
            var fileExtArray = SongExt /*.Concat(PlaylistExt).Concat(UniversalExt).ToArray()*/;

            var html = "<canvas id=\"canvas\" style=\"position: fixed; left: 0; top: 0; width: 100%; height: 100%;\">" +
                       "<audio id=\"audio\" preload=\"none\" style=\"visibility:hidden;\" controls autoplay></audio>" +
                       "<input id=\"select\" style=\"visibility:hidden;\" type=\"file\" accept=\"" +
                       string.Join(",", fileExtArray) + "\">" +
                       "</canvas>";

            WebAssemblyRuntime.InvokeJS($"document.getElementById('{element.HtmlId}').innerHTML = '{html}';");

            var timer = new DispatcherTimer();
            timer.Tick += TimerOnTick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            _isLoaded = true;

            if (!MainPage.UserAgent.Contains(MainPage.UserAgentPostfix)) return;

            element.RegisterHtmlCustomEventHandler("handleFile", OnHandleFile);
            WebAssemblyRuntime.InvokeJS(
                $"window.ipcRenderer.on('openFile', (event, args) => document.getElementById('{element.HtmlId}').dispatchEvent(new CustomEvent('handleFile', {{detail: args}})));");

            WebAssemblyRuntime.InvokeJS("window.ipcRenderer.send('engineLoaded');");
        }

        #endregion
    }
}