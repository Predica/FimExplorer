using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System;

namespace Predica.FimExplorer.UI.WPF
{
    /// <summary>
    /// Extracts text contained in Hyperlink contaning TextBlock, wrapped in another TextBlock
    /// </summary>
    public class HyperlinkTextExtractor
    {
        public string ExtractText(Hyperlink hyperlink)
        {
            TextBlock textBlock = (TextBlock)hyperlink.Parent;

            DependencyObject containerVisual = VisualTreeHelper.GetChild(textBlock, 0);
            DependencyObject contentPresenter = VisualTreeHelper.GetChild(containerVisual, 0);

            var innerTextBlock = contentPresenter as TextBlock;

            // true for link cells in main grid
            // false for link cells in details grid
            if (contentPresenter.GetType().IsNot<TextBlock>())
            {
                innerTextBlock = (TextBlock)VisualTreeHelper.GetChild(contentPresenter, 0);
            }

            return innerTextBlock.Text;
        }
    }
}