using System.IO;
using System.Windows;
using Microsoft.ResourceManagement.ObjectModel;
using NLog;
using Predica.FimCommunication.Import;
using Predica.FimExplorer.UI.WPF.Details;
using Predica.FimExplorer.UI.WPF.Import;

namespace Predica.FimExplorer.UI.WPF
{
    public class WindowsManager
    {
        public void ObjectDetailsDialog(RmResource target)
        {
            if (target == null)
            {
                _log.Debug("Cannot show details of null object");
                return;
            }

            var model = new ObjectDetailsModel(target, FimClientFactory.CreateClient(), this);

            var detailsWindow = new ObjectDetailsWindow();

            detailsWindow.Initialize(model);

            _log.Debug("Showing details of [{0}:{1}]", target.ObjectType, target.ObjectID);

            detailsWindow.Show();
        }

        public void ImportObjectsDialog(string fileName)
        {
            _log.Debug("Showing import window for file {0}", fileName);

            var importWindow = new ImportedObjectsWindow();

            ImportedObjectsModel model;
            using (var stream = File.OpenRead(fileName))
            {
                model = new ImportedObjectsModel(stream, new XmlImporter(), this);
                model.Initialize();
                importWindow.Initialize(model);
            }

            importWindow.ShowDialog();
        }

        public void Info(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    }
}