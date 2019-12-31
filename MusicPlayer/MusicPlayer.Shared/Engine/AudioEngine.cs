using System.Globalization;
using System.IO;
using File = MusicPlayer.Shared.Tools.File;

namespace MusicPlayer.Shared.Engine
{
    class AudioEngine
    {
        public void Select()
        {
            var stream = File.GetStreamFromResource(@"Select.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            Uno.Foundation.WebAssemblyRuntime.InvokeJS(function);
        }

        

        public void Play(string link = null)
        {
            var stream = File.GetStreamFromResource(@"Play.js", GetType());
            var reader = new StreamReader(stream);
            var function = link == null ? reader.ReadToEnd() : reader.ReadToEnd().Replace("URL.createObjectURL(input.files[0])", "\'" + link + "\'");

            Uno.Foundation.WebAssemblyRuntime.InvokeJS(function);
        }

        public void PausePlay()
        {
            var stream = File.GetStreamFromResource(@"PausePlay.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            Uno.Foundation.WebAssemblyRuntime.InvokeJS(function);
        }

        public void SetVolume(double volume)
        {
            var stream = File.GetStreamFromResource(@"setVolume.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd().Replace("$Volume", (volume / 100).ToString(CultureInfo.InvariantCulture));

            Uno.Foundation.WebAssemblyRuntime.InvokeJS(function);
        }

        public void SetPosition(double time)
        {
            var stream = File.GetStreamFromResource(@"setPosition.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd().Replace("$Time", time.ToString(CultureInfo.InvariantCulture));

            Uno.Foundation.WebAssemblyRuntime.InvokeJS(function);
        }

        public string GetFileName()
        {
            var stream = File.GetStreamFromResource(@"getFile.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            return Uno.Foundation.WebAssemblyRuntime.InvokeJS(function);
        }

        public bool IsPlaying()
        {
            var stream = File.GetStreamFromResource(@"isPlaying.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            return bool.Parse(Uno.Foundation.WebAssemblyRuntime.InvokeJS(function));
        }

        public double GetPosition()
        {
            var stream = File.GetStreamFromResource(@"getPosition.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            return double.Parse(Uno.Foundation.WebAssemblyRuntime.InvokeJS(function));
        }

        public double GetDuration()
        {
            var stream = File.GetStreamFromResource(@"getDuration.js", GetType());
            var reader = new StreamReader(stream);
            var function = reader.ReadToEnd();

            return double.Parse(Uno.Foundation.WebAssemblyRuntime.InvokeJS(function));
        }
    }
}
