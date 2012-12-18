using System.ComponentModel;
using System.Data;
using System.IO;
using Microsoft.ResourceManagement.ObjectModel;
using System.Linq;
using NLog;
using Predica.FimCommunication;
using Predica.FimCommunication.Import;

namespace Predica.FimExplorer.UI.WPF.Import
{
    public class ImportedObjectsModel : INotifyPropertyChanged
    {
        private readonly Stream _inputStream;
        private readonly IXmlImporter _xmlImporter;
        private readonly WindowsManager _windowsManager;

        private ImportResult _importResult;

        #region ImportedValues

        private DataTable _importedValues;
        public DataTable ImportedValues
        {
            get { return _importedValues; }
            set
            {
                _importedValues = value;
                NotifyChanged("ImportedValues");
            }
        }

        #endregion

        public ImportedObjectsModel(Stream inputStream, IXmlImporter xmlImporter, WindowsManager windowsManager)
        {
            _inputStream = inputStream;
            _xmlImporter = xmlImporter;
            _windowsManager = windowsManager;
        }

        public void Initialize()
        {
            _importResult = _xmlImporter.Import(_inputStream);

            if (_importResult.PrimaryImportObjects.IsEmpty())
            {
                return;
            }

            var table = new DataTable();

            var resourceWithMostAttributes = _importResult.PrimaryImportObjects.OrderByDescending(x => x.Attributes.Count).First();

            foreach (var attr in resourceWithMostAttributes.Attributes)
            {
                var column = table.Columns.Add(attr.Key.Name);

                // window can detect references by their 'object' type as opposed to 'string' defined for all other fields
                if (resourceWithMostAttributes[attr.Key.Name].Value is RmReference)
                {
                    column.DataType = typeof(object);
                }
                else
                {
                    column.DataType = typeof(string);
                }

            }

            foreach (var importTarget in _importResult.PrimaryImportObjects)
            {
                var newRowValues = table.Columns.Cast<DataColumn>().Select(x =>
                    {
                        if (importTarget.Attributes.Keys.Any(y => y.Name == x.ColumnName))
                        {
                            return importTarget.Attributes.First(y => y.Key.Name == x.ColumnName).Value.Value;
                        }
                        else
                        {
                            return null;
                        }
                    })
                    .ToArray();

                table.Rows.Add(newRowValues);
            }

            _log.Debug("Created {0}-column table with {1} imported rows", table.Columns.Count, table.Rows.Count);

            ImportedValues = table;
        }

        public void DetailsById(string id)
        {
            var obj = _importResult.AllImportedObjects.FirstOrDefault(x => x.ObjectID.Value == id);

            if (obj == null)
            {
                _windowsManager.Error("Object with id {0} was not imported".FormatWith(id));
            }
            else
            {
                _log.Debug("showing details of imported object {0}", id);
                _windowsManager.ObjectDetailsDialog(obj);
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    }
}