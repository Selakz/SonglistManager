using SonglistManager.Pages;

namespace SonglistManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
	public MainWindow()
	{
		InitializeComponent();
		MainFrame.Navigate(new MainEditPage());
		ChangeTitle();
	}

	private async Task ChangeTitle()
	{
		await Task.Delay(5000);
		Title = "SonglistManager";
	}
}