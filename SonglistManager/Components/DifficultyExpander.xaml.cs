using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using SonglistManager.Extensions;
using SonglistManager.Models;

namespace SonglistManager.Components;

public sealed partial class DifficultyExpander : UserControl
{
	public static readonly DependencyProperty RatingClassProperty =
		DependencyProperty.Register(nameof(RatingClass), typeof(int), typeof(DifficultyExpander),
			new PropertyMetadata(0, OnRatingClassChanged));

	public int RatingClass
	{
		get => (int)GetValue(RatingClassProperty);
		set => SetValue(RatingClassProperty, value);
	}

	private static void OnRatingClassChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is DifficultyExpander de)
			de.Expander.Header = de.GetHeader();
	}


	public SongInfo.Difficulty Difficulty { get; set; } = new();

	public DifficultyExpander()
	{
		InitializeComponent();
		Expander.Header = GetHeader();
	}

	public void UpdateForm(SongInfo.Difficulty difficulty)
	{
		RatingClass = difficulty.ratingClass;
		RatingPlus.IsChecked = difficulty.ratingPlus;
		Rating.Value = difficulty.rating;
		ChartDesigner.SetText(difficulty.chartDesigner);
		JacketDesigner.SetText(difficulty.jacketDesigner);
		TitleLocalized.Localization = difficulty.title_localized;
		Artist.Text = difficulty.artist;
		BpmBase.Value = difficulty.bpm_base;
		Bpm.Text = difficulty.bpm;
		Date.Value = difficulty.date;

		// Bg
		BgButton.UpdateByDifficulty(difficulty);

		Expander.Header = GetHeader();
		IsUseDiffToggle.IsOn = difficulty.HasDifference();
	}

	public SongInfo.Difficulty ExportDifficulty()
	{
		var chartDesigner = ChartDesigner.GetText();
		var jacketDesigner = JacketDesigner.GetText();

		SongInfo.Difficulty difficulty = new()
		{
			ratingClass = RatingClass,
			rating = (int)Rating.Value,
			ratingPlus = RatingPlus.IsChecked ?? false,
			chartDesigner = chartDesigner.Trim(),
			jacketDesigner = jacketDesigner.Trim(),
		};

		if (IsUseDiffToggle.IsOn)
		{
			difficulty.title_localized = TitleLocalized.Localization;
			difficulty.artist = Artist.Text;
			difficulty.jacketOverride = JacketOverride.IsOn;
			difficulty.audioOverride = AudioOverride.IsOn;
			difficulty.bpm_base = BpmBase.Value;
			difficulty.bpm = Bpm.Text;
			difficulty.bg = BgButton.BgName;
			difficulty.date = (int)Date.Value;

			difficulty.BackgroundPath = BgButton.BgPath;
		}

		return difficulty;
	}

	private string GetHeader()
	{
		if (RatingPlus != null && Rating != null)
			return
				$"{RatingClass}.aff {SongInfo.Difficulty.GetDifficultyString(RatingClass)} {(Rating.Value != 0 ? ((int)Rating.Value).ToString() : "?")}{(RatingPlus.IsChecked ?? false ? "+" : string.Empty)}";
		else return string.Empty;
	}

	private void Rating_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		Expander.Header = GetHeader();
	}

	private void RatingPlus_Click(object sender, RoutedEventArgs e)
	{
		Expander.Header = GetHeader();
	}
}