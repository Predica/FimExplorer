using System.Collections.Generic;
using System.Linq;
using Microsoft.ResourceManagement.ObjectModel;
using Predica.FimCommunication;

namespace Predica.FimExplorer.UI.WPF.Details
{
    public class ObjectDetailsModel
    {
        private readonly IFimClient _fimClient;
        private readonly WindowsManager _windowsManager;

        public RmResource Resource { get; set; }
        public IEnumerable<FlattenedAttribute> Attributes { get; set; }

        public string ResourceForWindowTitle
        {
            get
            {
                return "[{0}] {1} ({2})".FormatWith(Resource.ObjectType, Resource.DisplayName, Resource.ObjectID);
            }
        }

        public ObjectDetailsModel(RmResource resource, IFimClient fimClient, WindowsManager windowsManager)
        {
            _fimClient = fimClient;
            _windowsManager = windowsManager;

            Resource = resource;

            Attributes = resource.Attributes
                .Select(x => new FlattenedAttribute(x))
                .ToList();
        }

        public void ShowDetailsById(string id)
        {
            RmResource result = _fimClient.FindById(id);
            _windowsManager.ObjectDetailsDialog(result);
        }
    }
}