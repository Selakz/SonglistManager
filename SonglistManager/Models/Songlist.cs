using System.IO;
using Newtonsoft.Json;

namespace SonglistManager.Models;

public class Songlist
{
	public SongInfo[] songs { get; set; } = [];

	/// <summary> 尝试从给定路径的文件中读取Songlist，如果任何SongInfo能匹配到项目，则其source.Path是该项目的目录 </summary>
	public static Songlist? Load(string path)
	{
		List<SongInfo>? songInfos = null;
		FileInfo fileInfo = new(path);
		if (fileInfo.Exists)
		{
			string text = File.ReadAllText(path);
			// SongInfo
			try
			{
				var songInfo = JsonConvert.DeserializeObject<SongInfo>(text) ?? throw new Exception();
				if (string.IsNullOrEmpty(songInfo.id)) throw new Exception();
				songInfos = [songInfo];
			}
			// Songlist
			catch
			{
				try
				{
					var songlist = JsonConvert.DeserializeObject<Songlist>(text) ?? new();
					songInfos = [.. songlist.songs];
				}
				catch
				{
					return null;
				}
			}
		}

		// 没有解析出任何歌曲信息，返回null
		if (songInfos == null || songInfos.Count == 0) return null;
		// 如果只有一首歌，判断其所在文件夹本身是否是项目，如果不是则继续以多首歌处理
		var parent = fileInfo.Directory;
		if (parent == null) return null;
		if (songInfos.Count == 1)
		{
			ExplorerItem item = new(parent);
			if (item.Type is ExplorerItem.ProjectType.ArcadeClassic or ExplorerItem.ProjectType.ArcCreate)
			{
				songInfos[0].source = new()
				{
					Path = parent.FullName,
					Type = SongInfo.Source.SourceType.SonglistOfSingle
				};
				songInfos[0].BackgroundPath =
					Path.Combine(Setting.Instance.BackgroundFolderPath ?? string.Empty, $"{songInfos[0].bg}.jpg");
				return new() { songs = [.. songInfos] };
			}
		}

		// 如果不止一首歌，在所在目录里逐个找对应的项目
		var candidates = parent.GetDirectories();
		foreach (var songInfo in songInfos)
		{
			var names = songInfo.title_localized.GetNames();
			names.Add(songInfo.id);
			foreach (var candidate in candidates)
			{
				var candidateItem = new ExplorerItem(candidate);
				if (names.Any(name => name.Equals(candidate.Name, StringComparison.OrdinalIgnoreCase)))
				{
					if (candidateItem.Type is ExplorerItem.ProjectType.ArcadeClassic
					    or ExplorerItem.ProjectType.ArcCreate)
					{
						songInfo.source = new()
						{
							Path = candidate.FullName,
							Type = SongInfo.Source.SourceType.SonglistOfMulti
						};
						songInfo.BackgroundPath =
							Path.Combine(Setting.Instance.BackgroundFolderPath ?? string.Empty,
								$"{songInfos[0].bg}.jpg");
						break;
					}
				}

				songInfo.source = new()
				{
					Path = candidate.FullName,
					Type = SongInfo.Source.SourceType.WildSonglist
				};
			}
		}

		return new() { songs = [.. songInfos] };
	}
}