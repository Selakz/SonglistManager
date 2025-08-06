using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using SonglistManager.ArcCreate.Data;
using SonglistManager.ArcCreate.Storage;
using SonglistManager.Extensions;
using SonglistManager.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SonglistManager.ArcCreate.SonglistManager;

public class CustomExporter
{
	private readonly SongInfo? songInfo;
	private readonly Songlist? songlist;

	public CustomExporter(SongInfo songInfo)
	{
		this.songInfo = songInfo;
	}

	public CustomExporter(Songlist songlist)
	{
		this.songlist = songlist;
	}

	public List<string> Export(string outputPath)
	{
		List<string> errors = [];
		var serializer = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
			.Build();
		List<ImportInformation> infos = [];

		try
		{
			using (FileStream zipStream = new(outputPath, FileMode.Create))
			{
				var publisher = Setting.Instance.Publisher ?? string.Empty;
				using (ZipArchive zip = new(zipStream, ZipArchiveMode.Create, true))
				{
					if (songInfo is not null)
					{
						errors.AddRange(AddSongToZip(zip, songInfo));
						infos.Add(new()
						{
							Directory = songInfo.id,
							Identifier = $"{publisher}.{songInfo.id}",
							SettingsFile = $"{songInfo.id}.arcproj",
							Version = 1,
							Type = ImportInformation.LevelType
						});
					}

					if (songlist is not null)
					{
						foreach (var song in songlist.songs)
						{
							errors.AddRange(AddSongToZip(zip, song));
							infos.Add(new()
							{
								Directory = song.id,
								Identifier = $"{publisher}.{song.id}",
								SettingsFile = $"{song.id}.arcproj",
								Version = 1,
								Type = ImportInformation.LevelType
							});
						}
					}

					// 将List<ImportInformation>写入index.yml
					ZipArchiveEntry importInfoZipEntry = zip.CreateEntry(ImportInformation.FileName);
					using (Stream importInfoStream = importInfoZipEntry.Open())
					{
						using (StreamWriter writer = new StreamWriter(importInfoStream))
						{
							string infoYaml = serializer.Serialize(infos);
							writer.Write(infoYaml);
							writer.Flush();
						}
					}
				}
			}

			if (errors.Count > 0)
			{
				if (File.Exists(outputPath))
				{
					File.Delete(outputPath);
				}
			}
			else
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = Path.GetDirectoryName(outputPath),
					UseShellExecute = true,
					Verb = "open"
				});
			}
		}
		catch (Exception e)
		{
			if (File.Exists(outputPath))
			{
				File.Delete(outputPath);
			}

			errors.Add("导出过程中发生错误: " + e.Message);
		}

		return errors;
	}

	public static List<string> AddSongToZip(ZipArchive zip, SongInfo songInfo)
	{
		List<string> errors = [];

		string subdir = songInfo.id;
		ProjectSettings projSettings = new()
		{
			Charts = []
		};

		if (songInfo.source.Type is SongInfo.Source.SourceType.None or SongInfo.Source.SourceType.WildSonglist)
		{
			errors.Add($"{songInfo.id}无项目来源，无法导出");
			return errors;
		}

		// 添加谱面内容
		foreach (var difficulty in songInfo.difficulties)
		{
			if (!difficulty.IsEmpty())
			{
				// 谱面文件
				var chartPath = Path.Combine(songInfo.source.Path, $"{difficulty.ratingClass}.aff");
				if (File.Exists(chartPath))
				{
					var chartSettings = difficulty.GetChartSettings(songInfo);
					projSettings.Charts.Add(chartSettings);
					WriteFileToZip(zip, subdir, chartPath, $"{difficulty.ratingClass}.aff");
				}
				else
				{
					errors.Add($"找不到{songInfo.id}的{difficulty.ratingClass}.aff文件");
				}

				// 差分曲绘
				if (difficulty.jacketOverride)
				{
					var dJacketPath = Path.Combine(songInfo.source.Path, $"{difficulty.ratingClass}.jpg");
					if (File.Exists(dJacketPath))
					{
						WriteFileToZip(zip, subdir, dJacketPath, $"{difficulty.ratingClass}.jpg");
					}
					else
					{
						errors.Add($"找不到{songInfo.id}的差分曲绘：{difficulty.ratingClass}.jpg");
					}
				}

				// 差分音频
				if (difficulty.audioOverride)
				{
					var dMusicPath = Path.Combine(songInfo.source.Path, $"{difficulty.ratingClass}.ogg");
					if (File.Exists(dMusicPath))
					{
						WriteFileToZip(zip, subdir, dMusicPath, $"{difficulty.ratingClass}.ogg");
					}
					else
					{
						errors.Add($"找不到{songInfo.id}的差分音频：{difficulty.ratingClass}.ogg");
					}
				}

				// 差分背景
				if (!string.IsNullOrEmpty(difficulty.bg))
				{
					if (File.Exists(difficulty.BackgroundPath))
					{
						WriteFileToZip(zip, subdir, difficulty.BackgroundPath, $"{difficulty.ratingClass}_bg.jpg");
					}
					else errors.Add($"找不到{songInfo.id}的差分背景文件，请重新选择");
				}
			}
		}

		// 添加背景文件
		if (File.Exists(songInfo.BackgroundPath))
		{
			WriteFileToZip(zip, subdir, songInfo.BackgroundPath, "bg.jpg");
		}
		else errors.Add($"找不到{songInfo.id}的背景文件，请重新选择");

		// 添加曲绘文件
		var jacketPath = Path.Combine(songInfo.source.Path, "base.jpg");
		if (File.Exists(jacketPath))
		{
			WriteFileToZip(zip, subdir, jacketPath, "base.jpg");
		}
		else errors.Add($"找不到{songInfo.id}的曲绘文件，请检查");

		// 添加音频文件
		var musicPath = Path.Combine(songInfo.source.Path, "base.ogg");
		if (File.Exists(musicPath))
		{
			WriteFileToZip(zip, subdir, musicPath, "base.ogg");
		}
		else errors.Add($"找不到{songInfo.id}的音频文件，请检查");

		// 将ProjectSettings写入zip
		string projectZipPath = Path.Combine(subdir, $"{songInfo.id}.arcproj").Replace("\\", "/");
		ZipArchiveEntry projectZipEntry = zip.CreateEntry(projectZipPath);
		using Stream projectStream = projectZipEntry.Open();
		using StreamWriter writer = new StreamWriter(projectStream);
		var serializer = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
			.Build();
		var projYaml = serializer.Serialize(projSettings);
		writer.Write(projYaml);
		writer.Flush();

		return errors;
	}

	private static void WriteFileToZip(ZipArchive zip, string subdir, string sourcePath, string targetPath)
	{
		targetPath = Path.Combine(subdir, targetPath).Replace("\\", "/");
		ZipArchiveEntry entry = zip.CreateEntry(targetPath);
		using Stream entryStream = entry.Open();
		using FileStream fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
		fileStream.CopyTo(entryStream);
	}
}