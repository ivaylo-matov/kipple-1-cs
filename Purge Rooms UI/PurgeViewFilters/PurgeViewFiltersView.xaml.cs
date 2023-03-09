using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeViewFiltersView.xaml
    /// </summary>
    public partial class PurgeViewFiltersView : Window
    {
        UIDocument uidoc { get; set; }
        Document doc { get; set; }
        public PurgeViewFiltersView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge View Filters";
        }

        private void PurgeFiltersClick(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;
            using (Transaction t = new Transaction(doc, "Purge view filters"))
            {
                t.Start();

                if (chkFilters.IsChecked == true)
                {
                    foreach (ElementId id in PurgeViewFiltersCommand.GetUnusedViewFilters(doc))
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted++;
                        }
                        catch { }
                    }
                }
                t.Commit();
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No view filters were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 view filter was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} view filters were deleted."); }

        }
    }
}
