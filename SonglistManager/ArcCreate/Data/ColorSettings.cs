using System.Collections.Generic;

namespace SonglistManager.ArcCreate.Data
{
    public class ColorSettings
    {
        public string Trace { get; set; } = null;

        public string Shadow { get; set; } = null;

        public List<string> Arc { get; set; } = [];

        public List<string> ArcLow { get; set; } = new List<string>();
    }
}