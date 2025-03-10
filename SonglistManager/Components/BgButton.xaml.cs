using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SonglistManager.Models;

namespace SonglistManager.Components;

public partial class BgButton : UserControl
{
	public BgButton()
	{
		InitializeComponent();
	}

	public string BgPath
	{
		get => _bgPath;
		set
		{
			_bgPath = value;
			string fileName = Path.GetFileName(value);
			BgText.Text = fileName.Replace(".jpg", string.Empty);
		}
	}

	public string BgName => BgText.Text;

	private string _bgPath = string.Empty;

	public void UpdateByDifficulty(SongInfo.Difficulty difficulty)
	{
		BgPath = difficulty.BackgroundPath;
		BgText.Text = difficulty.bg;
	}

	private void BgButton_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button)
		{
			var setting = Setting.Instance;
			var dialog = new OpenFileDialog()
			{
				InitialDirectory = setting.BackgroundFolderPath,
				DefaultExt = ".jpg",
				Filter = "JPG Files (*.jpg)|*.jpg",
				Title = "Ñ¡Ôñ±³¾°ÎÄ¼þ",
				Multiselect = false
			};
			bool? result = dialog.ShowDialog();
			if (result == true)
			{
				BgPath = dialog.FileName;
			}
		}
	}
}