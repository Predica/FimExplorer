﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Predica.FimExplorer.UI.WPF
{
    /// <summary>
    /// Detects (based on type) if given autogenerated column is a reference
    /// and replaces it with hyperlink column with 'click' event handled
    /// </summary>
    public class ReferenceColumnDetector
    {
        public void ProcessAutogeneratedColumn<TReferenceType>(DataGridAutoGeneratingColumnEventArgs e, Action<string> handleReferenceClick)
        {
            var originalColumn = (DataGridBoundColumn)e.Column;
            Type propertyType = e.PropertyType;

            if (propertyType.Is<TReferenceType>())
            {
                var linkStyle = CreateLinkStyle(handleReferenceClick);

                e.Column = CreateHyperlinkColumn(originalColumn, linkStyle);
            }
        }

        private Style CreateLinkStyle(Action<string> handleReferenceClick)
        {
            var linkStyle = new Style(typeof (TextBlock));

            RoutedEventHandler clickHandler = (o, e) =>
                {
                    var textExtractor = new HyperlinkTextExtractor();
                    string id = textExtractor.ExtractText((Hyperlink) e.Source);
                    handleReferenceClick(id);
                };

            linkStyle.Setters.Add(new EventSetter(Hyperlink.ClickEvent, clickHandler));

            return linkStyle;
        }

        private static DataGridHyperlinkColumn CreateHyperlinkColumn(DataGridBoundColumn originalColumn, Style linkStyle)
        {
            var newColumn = new DataGridHyperlinkColumn
                {
                    ElementStyle = linkStyle,
                    Binding = originalColumn.Binding,
                    Header = originalColumn.Header,
                    HeaderStringFormat = "[ref] {0}",
                };

            return newColumn;
        }
    }
}