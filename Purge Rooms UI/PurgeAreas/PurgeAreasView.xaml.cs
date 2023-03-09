using Autodesk.Revit.UI;
using System.Windows;
using Autodesk.Revit.DB;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeAreasView.xaml
    /// </summary>
    public partial class PurgeAreasView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public PurgeAreasView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge Areas";
        }

        private void PurgeAreas(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;

            using (Transaction t = new Transaction(doc, "Purge areas"))
            {
                t.Start();

                if (chkPlaced.IsChecked == true)
                {
                    var unplacedAreaIds = PurgeAreasCommand.GetUnplacedAreaIds(doc);
                    foreach (ElementId id in unplacedAreaIds)
                    {
                        doc.Delete(id);
                        countDeleted += 1;
                    }
                }
                if (chkEnclosed.IsChecked == true)
                {
                    var unenclosedAreaIds = PurgeAreasCommand.GetUnenclosedAreaIds(doc);
                    foreach (ElementId id in unenclosedAreaIds)
                    {
                        doc.Delete(id);
                        countDeleted += 1;
                    }
                }
                t.Commit();
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No areas were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 area was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} areas were deleted."); }
        }
    }
}
