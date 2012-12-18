using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.ResourceManagement.Client;
using Microsoft.ResourceManagement.ObjectModel;
using Microsoft.ResourceManagement.ObjectModel.ResourceTypes;
using Predica.FimCommunication;
using Predica.FimCommunication.Export;
using Predica.FimCommunication.Querying;

namespace Predica.FimExplorer.UI.WPF.Main
{
    public class MainModel : INotifyPropertyChanged
    {
        private readonly MainWindow _parent;
        private readonly IFimClient _fimClient;
        private readonly WindowsManager _windowsManager;
        private readonly IXmlExporter _xmlExporter;

        #region Bound properties

        #region XPath

        private string _xPath;

        public string XPath
        {
            get { return _xPath; }
            set
            {
                _xPath = value;
                NotifyChanged("XPath");
            }
        }

        #endregion

        #region IdForSearch

        private string _idForSearch;

        public string IdForSearch
        {
            get { return _idForSearch; }
            set
            {
                _idForSearch = value;
                NotifyChanged("IdForSearch");
            }
        }

        #endregion

        #region QueriedValues

        private DataTable _queriedValues;

        public DataTable QueriedValues
        {
            get { return _queriedValues; }
            set
            {
                _queriedValues = value;
                NotifyChanged("QueriedValues");
            }
        }

        #endregion

        #region SelectedObjectType

        private RmObjectTypeDescription _selectedObjectType;

        public RmObjectTypeDescription SelectedObjectType
        {
            get { return _selectedObjectType; }
            set
            {
                _selectedObjectType = value;
                XPath = "/" + value.Name;
                FetchAttributesForSelectedObjectType();
            }
        }

        #endregion

        private DataRowView _selectedRow;
        public DataRowView SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                _selectedRow = value;
                IdForSearch = _selectedRow["Resource ID"].ToString();
            }
        }

        public ObservableCollection<RmObjectTypeDescription> ObjectTypes { get; private set; }
        public ObservableCollection<SelectableAttribute> CurrentAttributes { get; private set; }

        #endregion

        public MainModel(MainWindow parent, IFimClient fimClient, WindowsManager windowsManager, IXmlExporter xmlExporter)
        {
            _parent = parent;
            _fimClient = fimClient;
            _windowsManager = windowsManager;
            _xmlExporter = xmlExporter;

            ObjectTypes = new ObservableCollection<RmObjectTypeDescription>();
            CurrentAttributes = new ObservableCollection<SelectableAttribute>();
        }

        public void Initialize()
        {
            FetchObjectTypes();

            // cannot use ResourceTypeExtractor here - it is unable to extract type from RmResource
            SelectedObjectType = ObjectTypes.Single(x => x.Name == "Resource");
        }

        #region Fetching object types

        private void FetchObjectTypes()
        {
            _parent.LongRunningOperation(_parent.loadingIndicator, () =>
                {
                    var attributesToFetch = new AttributesToFetch(RmResource.AttributeNames.DisplayName.Name, RmAttributeTypeDescription.AttributeNames.Name.Name);

                    var list = _fimClient.EnumerateAll<RmObjectTypeDescription>("/ObjectTypeDescription", attributesToFetch)
                        .ToList();
                    ObjectTypes.Clear();
                    list.ForEach(x => ObjectTypes.Add(x));
                });
        }

        #endregion

        #region Fetching attributes

        private readonly Dictionary<string, List<SelectableAttribute>> _attributesCache =
            new Dictionary<string, List<SelectableAttribute>>();

        private void FetchAttributesForSelectedObjectType()
        {
            string typeName = SelectedObjectType.Name;

            FetchAttributesForObjectType(typeName);

            FillCurrentAttributesCollection(_attributesCache[typeName]);
        }

        private void FetchAttributesForObjectType(string typeName)
        {
            if (_attributesCache.ContainsKey(typeName))
            {
                return;
            }

            _parent.LongRunningOperation(_parent.loadingIndicator, () =>
                {
                    var attributesToFetch = new AttributesToFetch(

                        RmResource.AttributeNames.DisplayName.Name
                        , RmAttributeTypeDescription.AttributeNames.Name.Name
                        , RmAttributeTypeDescription.AttributeNames.DataType.Name
                        );

                    string query = "/BindingDescription[BoundObjectType = "
                                 + "/ObjectTypeDescription[Name = '{0}']]".FormatWith(typeName)
                                 + "/BoundAttributeType";

                    var attributes = _fimClient.EnumerateAll<RmAttributeTypeDescription>(query, attributesToFetch)
                        .ToList();

                    _attributesCache.Add(typeName, attributes.Select(x => new SelectableAttribute(x)).ToList());
                });
        }

        private void FillCurrentAttributesCollection(IEnumerable<SelectableAttribute> list)
        {
            CurrentAttributes.Clear();
            list.ForEach(x => CurrentAttributes.Add(x));
        }

        #endregion

        #region Executing query

        private bool querying_selected_type()
        {
            return XPath.StartsWith("/" + SelectedObjectType.Name);
        }

        public void ExecuteQuery()
        {
            var requiredAttributes = new List<SelectableAttribute>();

            // attributes selection is possible only when directly querying for type selected in the list
            // otherwise there is no smart/easy way to tell if queried object contains any of currently displayed attributes
            if (querying_selected_type())
            {
                if (CurrentAttributes.Any(x => x.IsSelected))
                {
                    requiredAttributes = CurrentAttributes.Where(x => x.IsSelected).ToList();
                }
                else
                {
                    requiredAttributes = CurrentAttributes.ToList();
                }

                if (requiredAttributes.None(x => x.Attribute.Name == RmResource.AttributeNames.ObjectID.Name))
                {
                    requiredAttributes.Add(CurrentAttributes.Single(x => x.Attribute.Name == RmResource.AttributeNames.ObjectID.Name));
                }
            }

            // empty array if querying for other type than selected => all attributes will be fetched
            var attributesToFetch = new AttributesToFetch(requiredAttributes.Select(x => x.Attribute.Name).ToArray());

            string query = XPath.Replace(Environment.NewLine, string.Empty);

            RmResource[] results = _parent.LongRunningOperation(_parent.loadingIndicator,
                () => _fimClient.EnumerateAll<RmResource>(query, attributesToFetch)
                    .ToArray()
                );

            DataTable table = new DataTable();

            if (results.IsNotEmpty())
            {
                // assuming that all results are of the same type
                var firstResult = results.First();
                var resultType = firstResult.GetResourceType();
                var resultTypeAttributes = firstResult.Attributes;

                SelectableAttribute[] fetchedAttributes;
                if (requiredAttributes.IsNotEmpty())
                {
                    fetchedAttributes = requiredAttributes.ToArray();
                }
                else
                {
                    FetchAttributesForObjectType(resultType);
                    fetchedAttributes = _attributesCache[resultType].ToArray();
                }

                var resultTableColumnNames = new List<string>();

                foreach (var a in resultTypeAttributes.OrderBy(x => x.Key.Name))
                {
                    var selectedAttribute = fetchedAttributes.SingleOrDefault(x => x.Attribute.Name == a.Key.Name);
                    if (selectedAttribute == null)
                    {
                        continue;
                    }

                    var column = table.Columns.Add(selectedAttribute.Attribute.DisplayName);
                    resultTableColumnNames.Add(selectedAttribute.Attribute.Name);

                    // window can detect references by their 'object' type as opposed to 'string' defined for all other fields
                    if (selectedAttribute.Attribute.DataType == RmFactory.RmAttributeType.Reference.ToString())
                    {
                        column.DataType = typeof(object);
                    }
                    else
                    {
                        column.DataType = typeof(string);
                    }
                }

                foreach (var resource in results)
                {
                    var newRowValues = resultTableColumnNames
                        .Select(x =>
                            {
                                if (resource.Attributes.Keys.Any(y => y.Name == x))
                                {
                                    return resource.Attributes.First(y => y.Key.Name == x)
                                        .Value.Value;
                                }
                                else
                                {
                                    return null;
                                }
                            })
                        .ToArray();

                    table.Rows.Add(newRowValues);
                }
            }

            QueriedValues = table;
        }

        #endregion

        #region Showing details by object id

        public void ShowDetailsById()
        {
            RmResource result = _parent.LongRunningOperation(_parent.loadingIndicator,
                () => _fimClient.FindById(IdForSearch)
            );
            _windowsManager.ObjectDetailsDialog(result);
        }

        #endregion

        public void ExportToXml(string filePath)
        {
            using (var fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                _xmlExporter.WriteXml(fileStream, XPath);
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}