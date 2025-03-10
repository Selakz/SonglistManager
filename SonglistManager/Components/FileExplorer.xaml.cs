using System.IO;
using System.Windows;
using System.Windows.Controls;
using SonglistManager.Models;

namespace SonglistManager.Components;

public partial class FileExplorer : UserControl
{
	public static string? RootPath
	{
		get
		{
			var setting = Setting.Instance;
			return setting.RootPath;
		}

		set
		{
			var setting = Setting.Instance;
			setting.RootPath = value;
			setting.Save();
		}
	}

	public event EventHandler<FileTreeViewItem> ButtonClicked = delegate { };

	public FileExplorer()
	{
		InitializeComponent();
		LoadFileSystem();
	}

	public void LoadFileSystem()
	{
		if (RootPath == null) return;
		List<FileTreeViewItem> dataSource = [];
		DirectoryInfo rootDir = new(RootPath);
		foreach (DirectoryInfo dir in rootDir.GetDirectories())
		{
			ExplorerItem item = new(dir);
			FileTreeViewItem fileTreeViewItem = new(item);
			fileTreeViewItem.Children.Add(new FileTreeViewItem());
			dataSource.Add(fileTreeViewItem);
		}

		foreach (FileInfo file in rootDir.GetFiles())
		{
			ExplorerItem item = new(file);
			FileTreeViewItem fileTreeViewItem = new(item);
			dataSource.Add(fileTreeViewItem);
		}

		dataSource.Sort((x, y) => x.Type.CompareTo(y.Type));
		FileTreeView.ItemsSource = dataSource;
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button { DataContext: FileTreeViewItem item })
		{
			ButtonClicked.Invoke(sender, item);
		}
	}

	private void TreeViewItem_OnExpanded(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is TreeViewItem { DataContext: FileTreeViewItem item })
		{
			if (item.Children.Count == 0) return;
			if (item.Children[0].Name == string.Empty)
			{
				// 清空占位符
				item.Children.Clear();

				try
				{
					DirectoryInfo baseDir = new(item.Path);
					// 加载子目录
					foreach (var dir in baseDir.GetDirectories())
					{
						ExplorerItem explorerItem = new(dir);
						var dirItem = new FileTreeViewItem(explorerItem);
						dirItem.Children.Add(new FileTreeViewItem()); // 添加一个空项以显示展开箭头
						item.Children.Add(dirItem);
					}

					// 加载子文件
					foreach (var file in baseDir.GetFiles())
					{
						ExplorerItem explorerItem = new(file);
						var fileItem = new FileTreeViewItem(explorerItem);
						item.Children.Add(fileItem);
					}
				}
				catch (UnauthorizedAccessException)
				{
					// 处理无权限访问的目录
					item.Children.Add(new FileTreeViewItem("Access Denied"));
				}
				catch (Exception ex)
				{
					// 处理其他异常
					item.Children.Add(new FileTreeViewItem(ex.GetType().ToString()));
				}
			}
		}
	}
}