using System.Windows;
using System.Windows.Controls;
using SonglistManager.Models;

namespace SonglistManager.Components;

public sealed partial class LocalizationTextBlock : UserControl
{
	public static readonly DependencyProperty PlaceholderTextProperty =
		DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(LocalizationTextBlock),
			new PropertyMetadata(string.Empty));

	public string PlaceholderText
	{
		get => (string)GetValue(PlaceholderTextProperty);
		set => SetValue(PlaceholderTextProperty, value);
	}

	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register(nameof(Header), typeof(string), typeof(LocalizationTextBlock),
			new PropertyMetadata(string.Empty));

	public string Header
	{
		get => (string)GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}


	private SongInfo.Localization localization = new();

	public SongInfo.Localization Localization
	{
		get => localization;
		set
		{
			localization = value;
			UpdateLocalization();
		}
	}

	public LocalizationTextBlock()
	{
		InitializeComponent();
	}

	private void UpdateLocalization()
	{
		if (LocalizationText is null) return;
		if (LanguageComboBox.SelectedItem is ComboBoxItem cbi)
		{
			LocalizationText.Text = cbi.Content.ToString() switch
			{
				"en" => Localization.en,
				"ja" => Localization.ja,
				"ko" => Localization.ko,
				"zh-Hans" => Localization.zh_Hans,
				"zh-Hant" => Localization.zh_Hant,
				_ => string.Empty
			} ?? string.Empty;
		}
	}

	private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		UpdateLocalization();
	}

	private void LocalizationText_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (LanguageComboBox.SelectedItem is ComboBoxItem cbi)
		{
			switch (cbi.Content.ToString())
			{
				case "en":
					localization.en = LocalizationText.Text;
					break;
				case "ja":
					localization.ja = LocalizationText.Text;
					break;
				case "ko":
					localization.ko = LocalizationText.Text;
					break;
				case "zh-Hans":
					localization.zh_Hans = LocalizationText.Text;
					break;
				case "zh-Hant":
					localization.zh_Hant = LocalizationText.Text;
					break;
			}
		}
	}
}