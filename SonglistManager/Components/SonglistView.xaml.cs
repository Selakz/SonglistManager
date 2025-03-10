using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using SonglistManager.ArcCreate.SonglistManager;
using SonglistManager.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace SonglistManager.Components;

public sealed partial class SonglistView : UserControl
{
	public ObservableCollection<SongInfo> SongInfos { get; } = [];

	public event EventHandler<SongInfo> SongInfoReEdited = delegate { };

	public SonglistView()
	{
		InitializeComponent();
		var setting = Setting.Instance;
		PublisherNameTextBox.Text = setting.Publisher ?? string.Empty;
	}

	private void MenuFlyoutItem_ReEdit(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem { DataContext: SongInfo songInfo })
		{
			SongInfoReEdited.Invoke(this, songInfo);
		}
	}

	private void MenuFlyoutItem_Delete(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem { DataContext: SongInfo songInfo })
		{
			SongInfos.Remove(songInfo);
		}
	}

	private void ExportAsFileButton_Click(object sender, RoutedEventArgs e)
	{
		if (SongInfos.Count == 0)
		{
			MessageBox.ShowAsync("一首歌都没有你是想导出什么啦...");
			return;
		}

		var dialog = new OpenFolderDialog()
		{
			Title = "选择文件夹",
			Multiselect = false
		};
		bool? result = dialog.ShowDialog();
		if (result == true)
		{
			var songlist = new Songlist
			{
				songs = SongInfos.ToArray()
			};
			string folderPath = dialog.FolderName;
			string json = JsonConvert.SerializeObject(songlist, Formatting.Indented);
			File.WriteAllText(Path.Combine(folderPath, "songlist"), json);
			Process.Start(new ProcessStartInfo
			{
				FileName = folderPath,
				UseShellExecute = true,
				Verb = "open"
			});
		}
	}

	private void ExportAsArcCreateButton_Click(object sender, RoutedEventArgs e)
	{
		if (SongInfos.Count == 0)
		{
			MessageBox.ShowAsync("一首歌都没有你是想导出什么啦...");
			return;
		}

		var dialog = new OpenFolderDialog()
		{
			Title = "选择文件夹",
			Multiselect = false
		};
		bool? result = dialog.ShowDialog();
		if (result == true)
		{
			var songlist = new Songlist
			{
				songs = SongInfos.ToArray()
			};
			string folderPath = dialog.FolderName;
			string fileName = $"ArcCreateSongBundle{DateTime.Now:MM-dd-HH-mm}.arcpkg";
			var errors = new CustomExporter(songlist).Export(Path.Combine(folderPath, fileName));
			if (errors.Count > 0)
			{
				string errorMessage = string.Join("\n - ", errors);
				MessageBox.ShowAsync($"操作失败，检测到以下错误：\n - {errorMessage}");
			}
		}
	}
}