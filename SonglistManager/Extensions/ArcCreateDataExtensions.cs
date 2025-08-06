using SonglistManager.ArcCreate.Data;
using SonglistManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonglistManager.Extensions
{
	public static class ArcCreateDataExtensions
	{
		public static SongInfo ToSongInfo(this ProjectSettings projectSettings)
		{
			if (projectSettings.Charts.Count > 0)
			{
				List<SongInfo.Difficulty> difficulties = [];
				bool allSameBpm =
					projectSettings.Charts.All(x => Math.Abs(x.BaseBpm - projectSettings.Charts[0].BaseBpm) < 0.01);
				bool allSameBpmText = projectSettings.Charts.All(x => x.BpmText == projectSettings.Charts[0].BpmText);
				bool allSameTitle = projectSettings.Charts.All(x => x.Title == projectSettings.Charts[0].Title);
				bool allSameComposer =
					projectSettings.Charts.All(x => x.Composer == projectSettings.Charts[0].Composer);
				foreach (var chartSettings in projectSettings.Charts)
				{
					var difficulty = chartSettings.ToDifficulty();
					if (allSameBpm) difficulty.bpm_base = 0;
					if (allSameBpmText) difficulty.bpm = string.Empty;
					if (allSameTitle) difficulty.title_localized = new();
					if (allSameComposer) difficulty.artist = string.Empty;
					difficulties.Add(difficulty);
				}

				return new()
				{
					id = allSameTitle ? SongInfo.TitleToId(projectSettings.Charts[0].Title) : string.Empty,
					title_localized = new() { en = allSameTitle ? projectSettings.Charts[0].Title : string.Empty },
					artist_localized = new()
						{ en = allSameComposer ? projectSettings.Charts[0].Composer : string.Empty },
					bpm_base = allSameBpm ? projectSettings.Charts[0].BaseBpm : 0,
					bpm = allSameBpmText ? projectSettings.Charts[0].BpmText : string.Empty,
					difficulties = [.. difficulties],
				};
			}

			return new();
		}

		public static SongInfo.Difficulty ToDifficulty(this ChartSettings chartSettings)
		{
			var (name, ratingString) = chartSettings.ParseDifficultyName(int.MaxValue);
			int ratingClass = name switch
			{
				"Past" => 0,
				"Present" => 1,
				"Future" => 2,
				"Beyond" => 3,
				"Eternal" => 4,
				_ => 2,
			};
			bool ratingPlus = false;
			if (ratingString.EndsWith('+'))
			{
				ratingString = ratingString[..^1];
				ratingPlus = true;
			}

			if (!int.TryParse(ratingString, out int rating)) rating = 0;
			return new()
			{
				title_localized = new() { en = chartSettings.Title ?? string.Empty },
				artist = chartSettings.Composer ?? string.Empty,
				chartDesigner = chartSettings.Charter ?? string.Empty,
				bpm_base = chartSettings.BaseBpm,
				bpm = chartSettings.BpmText ?? string.Empty,
				ratingClass = ratingClass,
				rating = rating,
				ratingPlus = ratingPlus,
			};
		}

		public static ProjectSettings ToProjectSettings(this SongInfo songInfo)
		{
			ProjectSettings proj = new()
			{
				Path = songInfo.source.Path,
				// Charts = songInfo.GetChartSettings(),
				LastOpenedChartPath = null
			};
			return proj;
		}

		public static ChartSettings GetChartSettings(this SongInfo.Difficulty d, SongInfo songInfo)
		{
			return new()
			{
				ChartPath = $"{d.ratingClass}.aff",
				AudioPath = d.audioOverride ? $"{d.ratingClass}.ogg" : "base.ogg",
				JacketPath = d.jacketOverride ? $"{d.ratingClass}.jpg" : "base.jpg",
				BaseBpm = d.bpm_base == 0 ? songInfo.bpm_base : d.bpm_base,
				BpmText = string.IsNullOrEmpty(d.bpm) ? songInfo.bpm : d.bpm,
				SyncBaseBpm = true,
				BackgroundPath = string.IsNullOrEmpty(d.bg) ? "bg.jpg" : $"{d.ratingClass}_bg.jpg",
				Title = string.IsNullOrEmpty(d.title_localized.en)
					? songInfo.title_localized.en
					: d.title_localized.en,
				Composer = string.IsNullOrEmpty(d.artist)
					? songInfo.artist_localized.en
					: d.artist,
				Charter = d.chartDesigner,
				Alias = string.Empty,
				Illustrator = d.jacketDesigner,
				Difficulty =
					$"{SongInfo.Difficulty.GetDifficultyString(d.ratingClass)} {(d.rating != 0 ? d.rating.ToString() : "?")}{(d.ratingPlus ? "+" : string.Empty)}",
				ChartConstant = d.rating + (d.ratingPlus ? 0.7 : 0),
				DifficultyColor = d.ratingClass switch
				{
					0 => "#3A6B78FF",
					1 => "#566947FF",
					2 => "#482B54FF",
					3 => "#7C1C30FF",
					4 => "#C960CCFF", // 自己捏的Eternal颜色
					_ => null
				},
				Skin = new()
				{
					Side = songInfo.side switch
					{
						0 => "light",
						1 => "conflict",
						2 => "colorless",
						_ => "colorless"
					},
					Particle = string.Empty,
					Track = string.Empty,
					SingleLine = "none"
				},
				PreviewStart = songInfo.audioPreview,
				PreviewEnd = songInfo.audioPreviewEnd
			};
		}
	}
}