using System.Windows;

namespace SonglistManager;

public partial class App
{
	public new static App? Current { get; private set; }

	public new MainWindow? MainWindow => (MainWindow?)base.MainWindow;

	public App()
	{
		Current = this;
		InitializeComponent();
	}
}