using System.Windows;
using System.Windows.Controls;
using Microsoft.ResourceManagement.ObjectModel;
using System;

namespace Predica.FimExplorer.UI.WPF.Details
{
    /// <summary>
    /// Selects hyperlink for references and normal text for other values
    /// </summary>
    public class DetailsValueCellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var contentPresenter = (ContentPresenter)container;
            var attribute = (FlattenedAttribute)item;

            if (attribute == null || attribute.ValueType.IsNot<RmReference>())
            {
                return (DataTemplate)contentPresenter.FindResource("normalCell");
            }
            else
            {
                return (DataTemplate)contentPresenter.FindResource("referenceCell");
            }
        }
    }
}