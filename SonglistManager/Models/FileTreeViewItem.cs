using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SonglistManager.Models;

public class FileTreeViewItem
{
	public ExplorerItem? SourceItem { get; }

	public FileTreeViewItem()
	{
		Name = string.Empty;
		Path = string.Empty;
	}

	public FileTreeViewItem(string name)
	{
		Name = name;
		Path = string.Empty;
	}

	public FileTreeViewItem(ExplorerItem explorerItem)
	{
		SourceItem = explorerItem;
		Name = explorerItem.Name;
		Path = explorerItem.Path;
	}

	public enum TreeViewType
	{
		Songlist,
		ArcadeClassic,
		ArcCreate,
		Folder,
		File,
	}

	public string Name { get; }
	public string Path { get; }
	public ObservableCollection<FileTreeViewItem> Children { get; set; } = [];

	public TreeViewType Type
	{
		get
		{
			if (SourceItem is null) return TreeViewType.File;
			try
			{
				if (!SourceItem.IsFolder)
				{
					return Name.StartsWith("songlist") ? TreeViewType.Songlist : TreeViewType.File;
				}
				else
				{
					DirectoryInfo dir = new(Path);
					if (dir.GetDirectories()
					    .Where(item => item.Name == "Arcade")
					    .SelectMany(item => item.GetFiles())
					    .Any(file => file.Name == "Project.arcade"))
					{
						return TreeViewType.ArcadeClassic;
					}

					if (dir.GetFiles()
					    .Any(item => item.Name.EndsWith(".arcproj")))
					{
						return TreeViewType.ArcCreate;
					}
				}
			}
			catch
			{
				return TreeViewType.File;
			}

			return TreeViewType.Folder;
		}
	}

	public string Icon => Type switch
	{
		TreeViewType.File => "\uE8A5",
		TreeViewType.Folder => "\uE8B7",
		TreeViewType.ArcadeClassic => "pack://application:,,,/Assets/Icons/ArcadePlus.ico",
		TreeViewType.ArcCreate => "pack://application:,,,/Assets/Icons/ArcCreate.ico",
		TreeViewType.Songlist => "pack://application:,,,/Assets/Icons/app.ico",
		_ => throw new NotImplementedException(),
	};

	public Visibility IsUseImage =>
		Type is TreeViewType.Songlist or TreeViewType.ArcadeClassic or TreeViewType.ArcCreate
			? Visibility.Visible
			: Visibility.Collapsed;

	public Visibility IsUseFontIcon =>
		Type is TreeViewType.File or TreeViewType.Folder ? Visibility.Visible : Visibility.Collapsed;
}