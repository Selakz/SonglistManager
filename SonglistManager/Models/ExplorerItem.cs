using SonglistManager.ArcCreate.Data;
using SonglistManager.Extensions;
using System.IO;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SonglistManager.Models
{
	public class ExplorerItem
	{
		public enum ProjectType
		{
			None,
			ArcadeClassic,
			ArcCreate,
			Songlist
		}

		public ExplorerItem(DirectoryInfo directoryInfo)
		{
			Name = directoryInfo.Name;
			Path = directoryInfo.FullName;
			IsFolder = true;
		}

		public ExplorerItem(FileInfo fileInfo)
		{
			Name = fileInfo.Name;
			Path = fileInfo.FullName;
			IsFolder = false;
		}

		public string Name { get; }
		public string Path { get; }
		public bool IsFolder { get; }

		public ProjectType Type
		{
			get
			{
				if (!IsFolder)
				{
					return Name.StartsWith("songlist") ? ProjectType.Songlist : ProjectType.None;
				}
				else
				{
					DirectoryInfo dir = new(Path);
					if (dir.GetDirectories()
					    .Where(item => item.Name == "Arcade")
					    .SelectMany(item => item.GetFiles())
					    .Any(file => file.Name == "Project.arcade"))
					{
						return ProjectType.ArcadeClassic;
					}

					if (dir.GetFiles()
					    .Any(item => item.Name.EndsWith(".arcproj")))
					{
						return ProjectType.ArcCreate;
					}
				}

				return ProjectType.None;
			}
		}

		public SongInfo ParseSongInfo()
		{
			try
			{
				SongInfo songInfo = new();
				return Type switch
				{
					ProjectType.None => songInfo,
					ProjectType.ArcadeClassic => ParseSongInfoFromArcadeClassic(new DirectoryInfo(Path)),
					ProjectType.ArcCreate => ParseSongInfoFromArcCreate(new DirectoryInfo(Path)),
					_ => songInfo
				};
			}
			catch
			{
				return new();
			}
		}

		private static SongInfo ParseSongInfoFromArcadeClassic(DirectoryInfo parentDirectory)
		{
			SongInfo songInfo = new()
			{
				source = new()
				{
					Type = SongInfo.Source.SourceType.FileExplorer,
					Path = parentDirectory.FullName
				}
			};
			string projPath = System.IO.Path.Combine(parentDirectory.FullName, "Arcade", "Project.arcade");
			JsonElement projInfo = JsonDocument.Parse(File.ReadAllText(projPath)).RootElement;
			// id, title_localized
			string title = projInfo.GetProperty("Title").GetString() ?? string.Empty;
			songInfo.title_localized = new() { en = title };
			songInfo.id = SongInfo.TitleToId(title);
			// artist, artist_localized
			songInfo.artist_localized = new() { en = projInfo.GetProperty("Artist").GetString() ?? string.Empty };
			// bpm, bpm_base
			songInfo.bpm_base = projInfo.GetProperty("BaseBpm").GetDouble();
			songInfo.bpm = songInfo.bpm_base.ToString("0.00");
			// difficulties
			JsonElement diffInfo = projInfo.GetProperty("Difficulties");
			if (diffInfo.ValueKind == JsonValueKind.Array)
			{
				List<SongInfo.Difficulty> difficulties = [];
				for (int i = 0; i < diffInfo.GetArrayLength(); i++)
				{
					JsonElement diffElement = diffInfo[i];
					if (diffElement.ValueKind != JsonValueKind.Object) continue;
					string ratingString = diffElement.TryGetProperty("Rating", out var ratingElement)
						? ratingElement.GetString() ?? string.Empty
						: string.Empty;
					bool ratingPlus = false;
					if (ratingString.EndsWith('+'))
					{
						ratingString = ratingString[..^1];
						ratingPlus = true;
					}

					var classString = SongInfo.Difficulty.GetDifficultyString(i);
					if (ratingString.StartsWith(classString))
					{
						ratingString = ratingString[classString.Length..];
					}

					if (!int.TryParse(ratingString, out int rating)) rating = 0;
					difficulties.Add(new()
					{
						ratingClass = i,
						rating = rating,
						ratingPlus = ratingPlus,
						title_localized = new()
						{
							en = diffElement.TryGetProperty("Title", out var d_title)
								? d_title.GetString() ?? string.Empty
								: string.Empty
						},
						chartDesigner = diffElement.TryGetProperty("ChartDesign", out var chartDesigner)
							? chartDesigner.GetString() ?? string.Empty
							: string.Empty,
						jacketDesigner = diffElement.TryGetProperty("Illustration", out var jacketDesigner)
							? jacketDesigner.GetString() ?? string.Empty
							: string.Empty,
					});
				}

				songInfo.difficulties = [.. difficulties];
			}

			// side
			songInfo.side = projInfo.TryGetProperty("SkinValues", out var skin)
				? skin.GetProperty("Tap").GetInt32()
				: 0;
			// date
			songInfo.date = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			return songInfo;
		}

		private static SongInfo ParseSongInfoFromArcCreate(DirectoryInfo parentDirectory)
		{
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.IgnoreUnmatchedProperties()
				.Build();
			var arcprojFile = parentDirectory.EnumerateFiles().First(file => file.Name.EndsWith(".arcproj"));
			var projectSettings = deserializer.Deserialize<ProjectSettings>(File.ReadAllText(arcprojFile.FullName));
			var result = projectSettings.ToSongInfo();
			result.source = new()
			{
				Type = SongInfo.Source.SourceType.FileExplorer,
				Path = parentDirectory.FullName
			};
			return result;
		}
	}
}