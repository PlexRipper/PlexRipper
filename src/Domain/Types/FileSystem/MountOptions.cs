using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class MountOptions
    {
        private readonly Dictionary<string, string> _options;

        public MountOptions(Dictionary<string, string> options)
        {
            _options = options;
        }

        public bool IsReadOnly => _options.ContainsKey("ro");
    }
}
