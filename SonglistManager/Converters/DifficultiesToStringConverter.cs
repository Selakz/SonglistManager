using System.Globalization;
using System.Windows.Data;
using SonglistManager.Models;

namespace SonglistManager.Converters;

public class DifficultiesToStringConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is SongInfo.Difficulty[] difficulties)
		{
			List<string> difficultiesStringList = [];
			difficultiesStringList.AddRange(difficulties.Select(difficulty =>
				$"{(difficulty.rating == 0 ? "?" : difficulty.rating.ToString())}{(difficulty.ratingPlus ? "+" : string.Empty)}"));
			return string.Join(" / ", difficultiesStringList);
		}

		return string.Empty;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}