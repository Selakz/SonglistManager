using Newtonsoft.Json;
using SonglistManager.Attributes;
using System.IO;

// ReSharper disable InconsistentNaming
namespace SonglistManager.Models
{
	[JsonConverter(typeof(CustomConverter<SongInfo>))]
	public class SongInfo
	{
		[JsonIgnore] public Source source { get; set; } = new();

		[NotRequired] public int idx { get; set; } = 0;
		public string id { get; set; } = string.Empty;
		public Localization title_localized { get; set; } = new();
		public Localization artist_localized { get; set; } = new();
		public string bpm { get; set; } = string.Empty;
		public double bpm_base { get; set; } = 0;
		public string set { get; set; } = "base";
		public string purchase { get; set; } = string.Empty;
		[NotRequired] public string category { get; set; } = string.Empty;
		public int audioPreview { get; set; } = 0;
		public int audioPreviewEnd { get; set; } = 0;
		public int side { get; set; } = 0;
		public string bg { get; set; } = string.Empty;
		[NotRequired] public string bg_inverse { get; set; } = string.Empty;
		[NotRequired] public BgDayNight bg_daynight { get; set; } = new();
		public int date { get; set; } = 0;
		[NotRequired] public string version { get; set; } = string.Empty;
		[NotRequired] public bool world_unlock { get; set; } = false;
		[NotRequired] public bool remote_dl { get; set; } = false;
		[NotRequired] public bool songlist_hidden { get; set; } = false;
		[NotRequired] public bool no_pp { get; set; } = false;
		[NotRequired] public Localization source_localized { get; set; } = new();
		[NotRequired] public string source_copyright { get; set; } = string.Empty;
		[NotRequired] public bool no_stream { get; set; } = false;
		[NotRequired] public Localization jacket_localized { get; set; } = new();
		public Difficulty[] difficulties { get; set; } = [];

		public class Source
		{
			public enum SourceType
			{
				None,
				FileExplorer,
				WildSonglist,
				SonglistOfSingle,
				SonglistOfMulti
			}

			public SourceType Type { get; init; } = SourceType.None;
			public string Path { get; init; } = string.Empty;
		}

		[JsonConverter(typeof(CustomConverter<Localization>))]
		public class Localization
		{
			public string en { get; set; } = string.Empty;
			[NotRequired] public string? ja { get; set; } = null;
			[NotRequired] public string? ko { get; set; } = null;

			[NotRequired]
			[JsonProperty("zh-Hans")]
			public string? zh_Hans { get; set; } = null;

			[NotRequired]
			[JsonProperty("zh-Hant")]
			public string? zh_Hant { get; set; } = null;

			public bool IsEmpty() => string.IsNullOrEmpty(en) && string.IsNullOrEmpty(ja) && string.IsNullOrEmpty(ko) &&
			                         string.IsNullOrEmpty(zh_Hans) && string.IsNullOrEmpty(zh_Hant);

			public List<string> GetNames()
			{
				var list = new List<string>();
				if (!string.IsNullOrEmpty(en)) list.Add(en);
				if (!string.IsNullOrEmpty(ja)) list.Add(ja);
				if (!string.IsNullOrEmpty(ko)) list.Add(ko);
				if (!string.IsNullOrEmpty(zh_Hans)) list.Add(zh_Hans);
				if (!string.IsNullOrEmpty(zh_Hant)) list.Add(zh_Hant);
				return list;
			}
		}

		[JsonConverter(typeof(CustomConverter<BgDayNight>))]
		public class BgDayNight
		{
			public string day { get; set; } = string.Empty;
			public string night { get; set; } = string.Empty;
		}

		[JsonConverter(typeof(CustomConverter<Difficulty>))]
		public class Difficulty
		{
			public int ratingClass { get; set; } = 0;
			public string chartDesigner { get; set; } = string.Empty;
			public string jacketDesigner { get; set; } = string.Empty;
			public int rating { get; set; } = 0;
			[NotRequired] public bool ratingPlus { get; set; } = false;
			[NotRequired] public bool legacy11 { get; set; } = false;
			[NotRequired] public bool plusFingers { get; set; } = false;
			[NotRequired] public string artist { get; set; } = string.Empty;
			[NotRequired] public string bpm { get; set; } = string.Empty;
			[NotRequired] public double bpm_base { get; set; } = 0;
			[NotRequired] public Localization title_localized { get; set; } = new();
			[NotRequired] public string jacket_night { get; set; } = string.Empty;
			[NotRequired] public bool jacketOverride { get; set; } = false;
			[NotRequired] public bool audioOverride { get; set; } = false;
			[NotRequired] public bool hidden_until_unlocked { get; set; } = false;
			[NotRequired] public string bg { get; set; } = string.Empty;
			[NotRequired] public string bg_inverse { get; set; } = string.Empty;
			[NotRequired] public bool world_unlock { get; set; } = false;
			[NotRequired] public int date { get; set; } = 0;
			[NotRequired] public string version { get; set; } = string.Empty;

			[JsonIgnore] public string BackgroundPath { get; set; } = string.Empty;

			public bool IsEmpty()
			{
				return rating == 0 && !ratingPlus && string.IsNullOrEmpty(chartDesigner);
			}

			public bool HasDifference()
			{
				return !(title_localized.IsEmpty() && string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(bpm) &&
				         bpm_base == 0 && !jacketOverride && !audioOverride && string.IsNullOrEmpty(bg) && date == 0);
			}

			public static string GetDifficultyString(int ratingClass)
			{
				string difficultyString = ratingClass switch
				{
					0 => "Past",
					1 => "Present",
					2 => "Future",
					3 => "Beyond",
					4 => "Eternal",
					_ => "Unknown"
				};
				return difficultyString;
			}
		}

		/// <summary> ���ַ���ת��ֻ����ĸ�����ֵ�Сд�ַ��������ת��ʧ���򷵻ؿ��ַ��� </summary>
		public static string TitleToId(string title)
		{
			string id = title.Replace(" ", "");
			bool canBeUsedAsId = true;
			foreach (var c in id)
				if (!(char.IsLetterOrDigit(c) && c < 127))
					canBeUsedAsId = false;
			if (!canBeUsedAsId) id = string.Empty;
			return id.ToLower();
		}

		public (List<string> errors, List<string> warnings) Validate()
		{
			var errors = new List<string>();
			var warnings = new List<string>();
			if (string.IsNullOrEmpty(id)) errors.Add("IDΪ��");
			if (string.IsNullOrEmpty(title_localized.en)) errors.Add("Ӣ�ı���Ϊ��");
			if (string.IsNullOrEmpty(artist_localized.en)) errors.Add("Ӣ����ʦ��Ϊ��");
			if (string.IsNullOrEmpty(bg)) errors.Add("����Ϊ��");
			if (difficulties.Length == 0) errors.Add("δ�����κ��Ѷ���Ϣ");

			if (bpm_base is <= 0 or double.NaN) warnings.Add("��׼BPMС�ڵ���0����ȷ����");
			if (string.IsNullOrEmpty(set)) warnings.Add("��������������Ϊ�գ�Ĭ������Ϊbase");
			if (audioPreview > audioPreviewEnd) warnings.Add("��ƵԤ������ʱ�䲻Ӧ���ڿ�ʼʱ��");
			if (date < 1e9) warnings.Add("dateӦΪ10λ����ʱ���");
			return (errors, warnings);
		}

		[JsonIgnore] public string BackgroundPath { get; set; } = string.Empty;

		[JsonIgnore]
		public string JacketPath
		{
			get
			{
				if (source.Type is Source.SourceType.FileExplorer or Source.SourceType.SonglistOfSingle
				    or Source.SourceType.SonglistOfMulti)
				{
					string jacketPath = Path.Combine(source.Path, "base.jpg");
					if (File.Exists(jacketPath)) return jacketPath;
				}

				return "pack://application:,,,/Assets/Icons/DefaultCover.png";
			}
		}

		[JsonIgnore]
		public string SourceDescription => source.Type switch
		{
			Source.SourceType.None => "����Ϣ��Դ�����ִ�",
			Source.SourceType.FileExplorer => "����Ϣ��Դ����Ŀ�ļ��н���",
			Source.SourceType.WildSonglist => "����Ϣ��Դ��songlist�ļ�",
			Source.SourceType.SonglistOfSingle => "����Ϣ��Դ����Ŀsonglist�ļ�",
			Source.SourceType.SonglistOfMulti => "����Ϣ��Դ������Ŀsonglist�ļ�",
			_ => "����Ϣ��Դ��Unknown"
		};
	}
}