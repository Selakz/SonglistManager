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
		SelectFolderButton.Content = FileExplorer.RootPath ?? "ѡ���ļ���";
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
			Title = "ѡ���ļ���",
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
		// ��ȡ��ǰ Pane �Ŀ��
		double newWidth = MainSplitView.OpenPaneLength + e.HorizontalChange;

		// ������С�����������
		if (newWidth < 100) newWidth = 100; // ��С���
		if (newWidth > 400) newWidth = 400; // �����

		// ���� Pane �Ŀ��
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
				Flyout("����ɹ�", true);
			}
			else
			{
				var songlist = Songlist.Load(item.Path);
				if (songlist == null)
				{
					Flyout("����ʧ�ܣ��ļ����ݸ�ʽ����ȷ", true);
					return;
				}

				if (songlist.songs.Length == 1)
				{
					SongInfoForm.UpdateForm(songlist.songs[0]);
					Flyout("����ɹ�", true);
				}
				else
				{
					FunctionView.SelectedItem = FunctionView.MenuItems[1];
					if (SonglistView.SongInfos.Count > 0)
					{
						FlyoutWithConfirm("��ǰSonglist����Ѵ�����Ŀ��Ϣ���Ƿ�ȷ�ϸ��ǣ�", () =>
						{
							SonglistView.SongInfos.Clear();
							foreach (var songInfo in songlist.songs)
							{
								SonglistView.SongInfos.Add(songInfo);
							}

							Flyout("���ǳɹ�", true);
						});
						return;
					}

					SonglistView.SongInfos.Clear();
					foreach (var songInfo in songlist.songs)
					{
						SonglistView.SongInfos.Add(songInfo);
					}

					Flyout("�ɹ����������Ŀ��Ϣ", true);
				}
			}
		}
	}

	// SongInfoForm�ĵ�������
	private void ValidateAndAction(SongInfo songInfo, Action action)
	{
		var (errors, warnings) = songInfo.Validate();
		if (errors.Count > 0)
		{
			string errorMessage = string.Join("\n - ", errors);
			string warningMessage = string.Join("\n - ", warnings);
			string message = $"����ʧ�ܣ���⵽���´���\n - {errorMessage}\n\n��⵽���½����޸ĵ����⣺\n - {warningMessage}";
			Flyout(message, false);
			return;
		}
		else if (warnings.Count > 0)
		{
			string warningMessage = string.Join("\n - ", warnings);
			string message = $"��⵽���½����޸ĵ����⣺\n - {warningMessage}\n\n�Ƿ����������";
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
						FlyoutWithConfirm("��⵽Songlist�д���ID��ͬ����Դ��ͬ����Ŀ���Ƿ�ȷ�ϸ��ǣ�", () =>
						{
							SonglistView.SongInfos[i] = songInfo;
							FunctionView.SelectedItem = FunctionView.MenuItems[1];
							Flyout("���ǳɹ�", true);
						});
						return;
					}

					SonglistView.SongInfos[i] = songInfo;
					FunctionView.SelectedItem = FunctionView.MenuItems[1];
					Flyout("�޸ĳɹ�", true);
					return;
				}
			}

			SonglistView.SongInfos.Add(songInfo);
			FunctionView.SelectedItem = FunctionView.MenuItems[1];
			Flyout("��ӳɹ�", true);
		});
	}

	private void ExportToPathButton_Clicked(object? sender, SongInfo songInfo)
	{
		ValidateAndAction(songInfo, () =>
		{
			var dialog = new OpenFolderDialog()
			{
				Title = "ѡ���ļ���",
				Multiselect = false
			};
			bool? result = dialog.ShowDialog();
			if (result == true)
			{
				string folderPath = dialog.FolderName;
				string json = JsonConvert.SerializeObject(songInfo, Formatting.Indented);
				File.WriteAllText(Path.Combine(folderPath, "songlist"), json);
				SongInfoForm.TryOpenFolder(folderPath);
				Flyout("�����ɹ�", true);
			}
		});
	}

	private void ExportToProjectButton_Clicked(object? sender, SongInfo songInfo)
	{
		if (songInfo.source.Type == SongInfo.Source.SourceType.None)
		{
			Flyout("�ñ�����Ŀ��Դ���޷�����", true);
			return;
		}

		ValidateAndAction(songInfo, () =>
		{
			string json = JsonConvert.SerializeObject(songInfo, Formatting.Indented);
			File.WriteAllText(Path.Combine(songInfo.source.Path, "songlist"), json);
			SongInfoForm.TryOpenFolder(songInfo.source.Path);
			Flyout("�����ɹ�", true);
		});
	}

	private void ExportAsArcCreateButton_Clicked(object? sender, SongInfo songInfo)
	{
		var setting = Setting.Instance;
		setting.Publisher = SongInfoForm.PublisherNameTextBox.Text;
		setting.Save();

		if (songInfo.source.Type == SongInfo.Source.SourceType.None)
		{
			Flyout("�ñ�����Ŀ��Դ���޷�����", false);
			return;
		}

		var dialog = new OpenFolderDialog()
		{
			Title = "ѡ���ļ���",
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
				Flyout($"����ʧ�ܣ���⵽���´���\n - {errorMessage}", false);
			}
		}
	}
}