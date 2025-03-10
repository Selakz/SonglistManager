using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SonglistManager.Models;

namespace SonglistManager.Components;

public sealed partial class SongInfoForm : UserControl
{
	public SongInfo DisplaySongInfo
	{
		get => ExportSongInfo();
		set => UpdateForm(value);
	}

	public event EventHandler<SongInfo> AddToSonglistClicked = delegate { };
	public event EventHandler<SongInfo> ExportToPathClicked = delegate { };
	public event EventHandler<SongInfo> ExportToProjectClicked = delegate { };
	public event EventHandler<SongInfo> ExportAsArcCreateClicked = delegate { };

	private SongInfo.Source Source { get; set; } = new() { Type = SongInfo.Source.SourceType.None };

	public SongInfoForm()
	{
		InitializeComponent();

		Date.Value = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		var setting = Setting.Instance;
		OpenFolderCheckBox.IsChecked = setting.IsOpenFolderPath;
		PublisherNameTextBox.Text = setting.Publisher ?? string.Empty;
	}

	public void UpdateForm(SongInfo songInfo)
	{
		Idx.Value = songInfo.idx;
		Id.Text = songInfo.id;
		TitleLocalized.Localization = songInfo.title_localized;
		ArtistLocalized.Localization = songInfo.artist_localized;
		Bpm.Text = songInfo.bpm;
		BpmBase.Value = songInfo.bpm_base;
		Set.Text = string.IsNullOrEmpty(songInfo.set) ? "base" : songInfo.set;
		Purchase.Text = songInfo.purchase;
		//// Category.Text = songInfo.category;
		AudioPreview.Value = songInfo.audioPreview;
		AudioPreviewEnd.Value = songInfo.audioPreviewEnd;
		Side.SelectedIndex = songInfo.side;
		BgInverse.Text = songInfo.bg_inverse;
		BgDay.Text = songInfo.bg_daynight.day;
		BgNight.Text = songInfo.bg_daynight.night;
		Date.Value = songInfo.date != 0 ? songInfo.date : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		//// Version.Text = songInfo.version;
		WorldUnlock.IsOn = songInfo.world_unlock;
		RemoteDl.IsOn = songInfo.remote_dl;
		SonglistHidden.IsOn = songInfo.songlist_hidden;
		//// NoPp.IsChecked = songInfo.no_pp;
		SourceLocalized.Localization = songInfo.source_localized;
		SourceCopyright.Text = songInfo.source_copyright;
		//// NoStream.IsChecked = songInfo.no_stream;
		//// JacketLocalized.Localization = songInfo.jacket_localized;

		// Bg
		BgButton.BgPath = songInfo.BackgroundPath;

		// Difficulties
		for (int i = 0; i < DifficultyGrid.Children.Count; i++)
		{
			var expander = DifficultyGrid.Children[i] as DifficultyExpander;
			expander?.UpdateForm(new() { ratingClass = i });
		}

		foreach (var difficulty in songInfo.difficulties)
		{
			if (difficulty.ratingClass >= 0 && difficulty.ratingClass < DifficultyGrid.Children.Count)
			{
				(DifficultyGrid.Children[difficulty.ratingClass] as DifficultyExpander)!.UpdateForm(difficulty);
			}
		}

		DiffcultyComboBox.SelectedIndex = songInfo.difficulties.Any(d => d.ratingClass == 3) ? 1 :
			songInfo.difficulties.Any(d => d.ratingClass == 4) ? 2 : 0;
		// Source
		Source = songInfo.source;
		SourceText.Text = songInfo.SourceDescription;
	}

	public SongInfo ExportSongInfo()
	{
		List<SongInfo.Difficulty> difficulties = [];
		foreach (var d in DifficultyGrid.Children)
		{
			if (d is DifficultyExpander expander)
			{
				var difficulty = expander.ExportDifficulty();
				if (difficulty.ratingClass is >= 0 and <= 2 ||
				    (DiffcultyComboBox.SelectedIndex == 1 && difficulty.ratingClass == 3) ||
				    (DiffcultyComboBox.SelectedIndex == 2 && difficulty.ratingClass == 4))
				{
					difficulties.Add(difficulty);
				}
			}
		}

		return new()
		{
			source = Source,
			idx = Idx.Value >= 0 ? (int)Idx.Value : 0,
			id = Id.Text,
			title_localized = TitleLocalized.Localization,
			artist_localized = ArtistLocalized.Localization,
			bpm = Bpm.Text,
			bpm_base = BpmBase.Value,
			set = Set.Text,
			purchase = Purchase.Text,
			// category = Category.Text,
			audioPreview = (int)AudioPreview.Value,
			audioPreviewEnd = (int)AudioPreviewEnd.Value,
			side = Side.SelectedIndex,
			bg = BgButton.BgName,
			bg_inverse = BgInverse.Text,
			bg_daynight = new() { day = BgDay.Text, night = BgNight.Text },
			date = (int)Date.Value,
			// version = Version.Text,
			world_unlock = WorldUnlock.IsOn,
			remote_dl = RemoteDl.IsOn,
			songlist_hidden = SonglistHidden.IsOn,
			// no_pp = NoPp.IsChecked,
			source_localized = SourceLocalized.Localization,
			source_copyright = SourceCopyright.Text,
			// no_stream = NoStream.IsChecked,
			// jacket_localized = JacketLocalized.Localization,
			difficulties = [.. difficulties],

			BackgroundPath = BgButton.BgPath
		};
	}

	public void TryOpenFolder(string path)
	{
		if (OpenFolderCheckBox.IsChecked ?? false)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true,
				Verb = "open"
			});
		}
	}

	private void CopyrightTextBox_GettingFocus(object o, RoutedEventArgs routedEventArgs)
	{
		if (o is TextBox { Text: "" } tb)
		{
			tb.Text = "©";
		}
	}

	private void AddToSonglistButton_Click(object sender, RoutedEventArgs e)
	{
		AddToSonglistClicked.Invoke(sender, DisplaySongInfo);
	}

	private void ExportToPathButton_Click(object sender, RoutedEventArgs e)
	{
		ExportToPathClicked.Invoke(sender, DisplaySongInfo);
	}

	private void ExportToProjectButton_Click(object sender, RoutedEventArgs e)
	{
		ExportToProjectClicked.Invoke(sender, DisplaySongInfo);
	}

	private void ExportAsArcCreateButton_Click(object sender, RoutedEventArgs e)
	{
		ExportAsArcCreateClicked.Invoke(sender, DisplaySongInfo);
	}

	private void OpenFolderCheckBox_Click(object sender, RoutedEventArgs e)
	{
		var setting = Setting.Instance;
		setting.IsOpenFolderPath = OpenFolderCheckBox.IsChecked ?? true;
		setting.Save();
	}
}