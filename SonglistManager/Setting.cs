using System.IO;
using Newtonsoft.Json;

namespace SonglistManager;

public class Setting
{
	private const string SettingsFileName = "slst-settings.json";

	private Setting()
	{
	}

	private static Setting? _instance;

	public static Setting Instance
	{
		get
		{
			if (_instance != null) return _instance;
			if (File.Exists(SettingsFileName))
			{
				try
				{
					var read = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(SettingsFileName));
					if (read != null) _instance = read;
					else
					{
						File.WriteAllText(SettingsFileName, "{}");
						_instance = new Setting();
					}
				}
				catch
				{
					File.WriteAllText(SettingsFileName, "{}");
					_instance = new Setting();
				}
			}
			else
			{
				File.WriteAllText(SettingsFileName, "{}");
				_instance = new Setting();
			}

			return _instance;
		}
	}

	public string? RootPath { get; set; }

	public bool IsOpenFolderPath { get; set; } = true;

	public string? BackgroundFolderPath { get; set; }

	public string? Publisher { get; set; }

	public void Save()
	{
		File.WriteAllText(SettingsFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
	}
}