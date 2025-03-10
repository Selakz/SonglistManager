using System.Collections.Generic;

namespace SonglistManager.ArcCreate.Data
{
	public class EditorProjectSettings
	{
		public string? LastUsedPublisher { get; set; }

		public string? LastUsedPackageName { get; set; }

		public int LastUsedVersionNumber { get; set; }

		public Dictionary<string, List<Timestamp>>? Timestamps { get; set; }
	}
}