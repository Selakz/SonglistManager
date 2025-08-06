using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using SonglistManager.ArcCreate.SonglistManager;
using SonglistManager.Components;
using SonglistManager.Models;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Page = System.Windows.Controls.Page;

namespace SonglistManager.Pages;

public partial class MainEditPage : Page
{
	public MainEditPage()
	{
		InitializeComponent();
		SelectFolderButton.Content = FileExplorer.RootPath ?? "选择文件夹";
		FileTreeView.ButtonClicked += FileTreeView_ButtonClicked;
		SongInfoForm.AddToSonglistClicked += AddToSonglistButton_Clicked;
		SongInfoForm.ExportToPathClicked += ExportToPathButton_Clicked;
		SongInfoForm.ExportToProjectClicked += ExportToProjectButton_Clicked;
		SongInfoForm.ExportAsArcCreateClicked += ExportAsArcCreateButton_Clicked;
		SonglistView.SongInfoReEdited += (_, songInfo) =>
		{
			SongInfoForm.UpdateForm(songInfo);
			FunctionView.SelectedItem = FunctionView.MenuItems[0];
		};
	}

	public async void Flyout(string message, bool isAutoClose)
	{
		if (isAutoClose)
		{
			InfoFlyout.Subtitle = message;
			InfoFlyout.IsOpen = true;
			await Task.Delay(1000);
			InfoFlyout.IsOpen = false;
		}
		else
		{
			await MessageBox.ShowAsync(message);
		}
	}

	public static async void FlyoutWithConfirm(string message, Action confirmAction)
	{
		var result = await MessageBox.ShowAsync(message, "", MessageBoxButton.YesNo);
		if (result == MessageBoxResult.Yes) confirmAction();
	}

	private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
	{
		var dialog = new OpenFolderDialog()
		{
			Title = "选择文件夹",
			Multiselect = false
		};
		bool? result = dialog.ShowDialog();
		if (result == true)
		{
			string folderPath = dialog.FolderName;
			SelectFolderButton.Content = folderPath;
			FileExplorer.RootPath = folderPath;
			FileTreeView.LoadFileSystem();
		}
	}

	private void SelectFolderButton_RightTapped(object sender, MouseButtonEventArgs e)
	{
		if (sender is Button button)
		{
			var path = button.Content as string ?? string.Empty;
			DirectoryInfo di = new(path);
			if (di.Exists)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = path,
					UseShellExecute = true,
					Verb = "open"
				});
			}
		}
	}

	private void PaneResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
	{
		// 获取当前 Pane 的宽度
		double newWidth = MainSplitView.OpenPaneLength + e.HorizontalChange;

		// 设置最小和最大宽度限制
		if (newWidth < 100) newWidth = 100; // 最小宽度
		if (newWidth > 400) newWidth = 400; // 最大宽度

		// 更新 Pane 的宽度
		MainSplitView.OpenPaneLength = newWidth;
	}

	private void TogglePaneButton_Click(object sender, RoutedEventArgs e)
	{
		MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
	}

	private void FunctionView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		SongInfoForm.Visibility = Visibility.Collapsed;
		SonglistView.Visibility = Visibility.Collapsed;

		var selectedItem = args.SelectedItem as NavigationViewItem;
		var choice = selectedItem?.Tag.ToString();
		switch (choice)
		{
			case "SongInfo":
				SongInfoForm.Visibility = Visibility.Visible;
				break;
			case "Songlist":
				SonglistView.Visibility = Visibility.Visible;
				break;
		}
	}

	private void FileTreeView_ButtonClicked(object? sender, FileTreeViewItem e)
	{
		MainSplitView.IsPaneOpen = false;
		var item = e.SourceItem;
		if (item is not null)
		{
			if (item.Type != ExplorerItem.ProjectType.Songlist)
			{
				var songInfo = item.ParseSongInfo();
				SongInfoForm.UpdateForm(songInfo);
				Flyout("导入成功", true);
			}
			else
			{
				var songlist = Songlist.Load(item.Path);
				if (songlist == null)
				{
					Flyout("导入失败，文件内容格式不正确", true);
					return;
				}

				if (songlist.songs.Length == 1)
				{
					SongInfoForm.UpdateForm(songlist.songs[0]);
					Flyout("导入成功", true);
				}
				else
				{
					FunctionView.SelectedItem = FunctionView.MenuItems[1];
					if (SonglistView.SongInfos.Count > 0)
					{
						FlyoutWithConfirm("当前Songlist面板已存在曲目信息，是否确认覆盖？", () =>
						{
							SonglistView.SongInfos.Clear();
							foreach (var songInfo in songlist.songs)
							{
								SonglistView.SongInfos.Add(songInfo);
							}

							Flyout("覆盖成功", true);
						});
						return;
					}

					SonglistView.SongInfos.Clear();
					foreach (var songInfo in songlist.songs)
					{
						SonglistView.SongInfos.Add(songInfo);
					}

					Flyout("成功导入多首曲目信息", true);
				}
			}
		}
	}

	// SongInfoForm的导出部分
	private void ValidateAndAction(SongInfo songInfo, Action action)
	{
		var (errors, warnings) = songInfo.Validate();
		if (errors.Count > 0)
		{
			string errorMessage = string.Join("\n - ", errors);
			string warningMessage = string.Join("\n - ", warnings);
			string message = $"操作失败，检测到以下错误：\n - {errorMessage}\n\n检测到以下建议修改的问题：\n - {warningMessage}";
			Flyout(message, false);
			return;
		}
		else if (warnings.Count > 0)
		{
			string warningMessage = string.Join("\n - ", warnings);
			string message = $"检测到以下建议修改的问题：\n - {warningMessage}\n\n是否继续操作？";
			FlyoutWithConfirm(message, action);
			return;
		}

		action();
	}

	private void AddToSonglistButton_Clicked(object? sender, SongInfo songInfo)
	{
		ValidateAndAction(songInfo, () =>
		{
			for (int i = 0; i < SonglistView.SongInfos.Count; i++)
			{
				if (SonglistView.SongInfos[i].id == songInfo.id)
				{
					if (SonglistView.SongInfos[i].source.Type != songInfo.source.Type)
					{
						FlyoutWithConfirm("检测到Songlist中存在ID相同但来源不同的曲目，是否确认覆盖？", () =>
						{
							SonglistView.SongInfos[i] = songInfo;
							FunctionView.SelectedItem = FunctionView.MenuItems[1];
							Flyout("覆盖成功", true);
						});
						return;
					}

					SonglistView.SongInfos[i] = songInfo;
					FunctionView.SelectedItem = FunctionView.MenuItems[1];
					Flyout("修改成功", true);
					return;
				}
			}

			SonglistView.SongInfos.Add(songInfo);
			FunctionView.SelectedItem = FunctionView.MenuItems[1];
			Flyout("添加成功", true);
		});
	}

	private void ExportToPathButton_Clicked(object? sender, SongInfo songInfo)
	{
		ValidateAndAction(songInfo, () =>
		{
			var dialog = new OpenFolderDialog()
			{
				Title = "选择文件夹",
				Multiselect = false
			};
			bool? result = dialog.ShowDialog();
			if (result == true)
			{
				string folderPath = dialog.FolderName;
				string json = JsonConvert.SerializeObject(songInfo, Formatting.Indented);
				File.WriteAllText(Path.Combine(folderPath, "songlist"), json);
				SongInfoForm.TryOpenFolder(folderPath);
				Flyout("导出成功", true);
			}
		});
	}

	private void ExportToProjectButton_Clicked(object? sender, SongInfo songInfo)
	{
		if (songInfo.source.Type == SongInfo.Source.SourceType.None)
		{
			Flyout("该表单无项目来源，无法导出", true);
			return;
		}

		ValidateAndAction(songInfo, () =>
		{
			string json = JsonConvert.SerializeObject(songInfo, Formatting.Indented);
			File.WriteAllText(Path.Combine(songInfo.source.Path, "songlist"), json);
			SongInfoForm.TryOpenFolder(songInfo.source.Path);
			Flyout("导出成功", true);
		});
	}

	private void ExportAsArcCreateButton_Clicked(object? sender, SongInfo songInfo)
	{
		var setting = Setting.Instance;
		setting.Publisher = SongInfoForm.PublisherNameTextBox.Text;
		setting.Save();

		if (songInfo.source.Type == SongInfo.Source.SourceType.None)
		{
			Flyout("该表单无项目来源，无法导出", false);
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
			string folderPath = dialog.FolderName;
			var targetPath = Path.Combine(folderPath, $"{songInfo.id}.arcpkg");
			var errors = new CustomExporter(songInfo).Export(targetPath);
			if (errors.Count > 0)
			{
				string errorMessage = string.Join("\n - ", errors);
				Flyout($"操作失败，检测到以下错误：\n - {errorMessage}", false);
			}
		}
	}
}