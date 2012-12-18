using System.Windows;
using System.Windows.Documents;

namespace Predica.FimExplorer.UI.WPF.Details
{
    public partial class ObjectDetailsWindow : Window
    {
        private ObjectDetailsModel _model;

        public ObjectDetailsWindow()
        {
            InitializeComponent();
        }

        public void Initialize(ObjectDetailsModel model)
        {
            _model = model;
            this.DataContext = model;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var textExtractor = new HyperlinkTextExtractor();
            string id = textExtractor.ExtractText((Hyperlink) e.Source);
            _model.ShowDetailsById(id);
        }

        private void ValueColumn_CopyingCellClipboardContent(object sender, System.Windows.Controls.DataGridCellClipboardEventArgs e)
        {
            var attribute = e.Item as FlattenedAttribute;

            if (attribute == null)
            {
                return;
            }

            e.Content = attribute.Value;
        }
    }
}
