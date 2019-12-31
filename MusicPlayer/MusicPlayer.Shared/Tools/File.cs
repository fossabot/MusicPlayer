using System;
using System.IO;

namespace MusicPlayer.Shared.Tools
{
    class File
    {
        public static Stream GetStreamFromResource(string resourceName, Type typeCalling)
        {
            try
            {
                var assy = typeCalling?.Assembly;
                var resources = assy?.GetManifestResourceNames();
                foreach (var sResourceName in resources)
                {
                    if (sResourceName.ToUpperInvariant().EndsWith(resourceName.ToUpperInvariant(), StringComparison.InvariantCulture))
                    {
                        return assy?.GetManifestResourceStream(sResourceName);
                    }
                }
            }
            catch
            {
                //
            }

            throw new Exception("Unable to find resource file:" + resourceName);
        }
    }
}
