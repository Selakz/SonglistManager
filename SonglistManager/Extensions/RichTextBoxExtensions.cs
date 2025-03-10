using System.Windows.Controls;
using System.Windows.Documents;

namespace SonglistManager.Extensions;

public static class RichTextBoxExtensions
{
	public static string GetText(this RichTextBox richTextBox)
	{
		TextRange textRange = new TextRange(
			richTextBox.Document.ContentStart,
			richTextBox.Document.ContentEnd
		);

		// 获取纯文本
		return textRange.Text;
	}

	public static void SetText(this RichTextBox richTextBox, string text)
	{
		TextRange textRange = new(
			richTextBox.Document.ContentStart,
			richTextBox.Document.ContentEnd
		)
		{
			Text = text
		};
	}
}