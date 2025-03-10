using System.Collections.Generic;

namespace SonglistManager.ArcCreate.Storage
{
    public class PackImportInformation
    {
        public string PackName { get; set; }

        public string ImagePath { get; set; }

        public List<string> LevelIdentifiers { get; set; }
    }
}
