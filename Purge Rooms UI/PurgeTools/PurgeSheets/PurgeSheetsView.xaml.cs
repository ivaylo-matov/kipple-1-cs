using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeSheetsView.xaml
    /// </summary>
    public partial class PurgeSheetsView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public PurgeSheetsView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge Sheets";
        }

        private void PurgeSheets(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;
            if (chkPurgeSheets.IsChecked == true)
            {
                using (Transaction t = new Transaction(doc, "Purge Unused Sheets"))
                {
                    t.Start();
                    var unusedSheetsIds = new List<ElementId>();
                    var allSheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToList();
                    foreach (ViewSheet sheet in allSheets)
                    {
                        if (sheet.GetAllViewports().Count() == 0 && sheet.GetAllRevisionIds().Count() == 0)
                        {
                            try
                            {
                                doc.Delete(sheet.Id);
                                countDeleted += 1;

                            }
                            catch { }
                        }
                    }
                    t.Commit();
                }
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No sheets were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 sheet was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} sheets were deleted."); }
        }
    }
}
