using System;
using System.Collections.Generic;
using Microsoft.ResourceManagement.ObjectModel;

namespace Predica.FimExplorer.UI.WPF.Details
{
    public class FlattenedAttribute
    {
        private readonly KeyValuePair<RmAttributeName, RmAttributeValue> _attributeDescrpition;

        public string AttributeName { get { return _attributeDescrpition.Key.Name; } }
        public string Value
        {
            get
            {
                return _attributeDescrpition.Value.Value == null 
                    ? string.Empty 
                    : _attributeDescrpition.Value.Value.ToString();
            }
        }
        public Type ValueType
        {
            get
            {
                return _attributeDescrpition.Value.Value == null 
                    ? null
                    : _attributeDescrpition.Value.Value.GetType();
            }
        }
        public string ValueTypeName
        {
            get
            {
                return _attributeDescrpition.Value.Value == null 
                    ? string.Empty 
                    : _attributeDescrpition.Value.Value.GetType().Name;
            }
        }

        public FlattenedAttribute(KeyValuePair<RmAttributeName, RmAttributeValue> attributeDescrpition)
        {
            _attributeDescrpition = attributeDescrpition;
        }
    }
}