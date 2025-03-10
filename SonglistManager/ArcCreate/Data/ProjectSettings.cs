using SonglistManager.Models;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SonglistManager.ArcCreate.Data
{
	public class ProjectSettings
	{
		[YamlIgnore] public string? Path { get; set; }

		public string? LastOpenedChartPath { get; set; }

		public List<ChartSettings>? Charts { get; set; }

		public EditorProjectSettings? EditorSettings { get; set; }
	}
}