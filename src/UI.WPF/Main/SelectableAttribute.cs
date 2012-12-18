using Microsoft.ResourceManagement.ObjectModel.ResourceTypes;

namespace Predica.FimExplorer.UI.WPF.Main
{
    public class SelectableAttribute
    {
        public RmAttributeTypeDescription Attribute { get; set; }
        public bool IsSelected { get; set; }

        public SelectableAttribute(RmAttributeTypeDescription attribute)
        {
            Attribute = attribute;
        }
    }
}