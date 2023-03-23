using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeCADImports.xaml
    /// </summary>
    public partial class PurgeCADImportsView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public PurgeCADImportsView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge CAD Imports";
        }

        private void PurgeCADs(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;
            using (Transaction t = new Transaction(doc, "Purge CAD Imports"))
            {
                t.Start();
                try
                {
                    if (chkCADs.IsChecked == true)
                    {
                        var importedCADs = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).ToList();
                        foreach (ImportInstance ii in importedCADs)
                        {
                            if (ii.IsLinked == false)
                            {
                                doc.Delete(ii.Id);
                                countDeleted += 1;
                            }
                        }
                    }
                    else { TaskDialog.Show("User Error", "Please check at least one box."); }
                }
                catch (System.Exception)
                {
                    // do nothing ?
                }
                t.Commit();
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No imported CAD files were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 imported CAD file was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} Imported CAD files were deleted."); }
        }
    }
}
