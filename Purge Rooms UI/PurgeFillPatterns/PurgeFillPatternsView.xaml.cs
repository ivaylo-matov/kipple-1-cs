using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace Purge_Rooms_UI
{
     public partial class PurgeFillPatternsView : Window
    {
        public UIDocument uidoc { get; }

        public Document doc { get; }

        public PurgeFillPatternsView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge Fill Patterns";
        }

        private void PurgeFillPatterns(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;

            using (Transaction t = new Transaction(doc, "Purge Fill Regions"))
            {
                t.Start();

                var idsToDetele = new List<ElementId>();
                var unusedFillPatternIds = PurgeFillPatternsCommand.GetUnusedFillPatternIds(doc);
                var CADFillPatternIds = PurgeFillPatternsCommand.GetCADFillPatternIds(doc);

                if (chkUnusedPatterns.IsChecked == true && chkCADPat.IsChecked == true)
                {
                    foreach (ElementId id in unusedFillPatternIds)
                    {
                        idsToDetele.Add(id);
                    }
                    foreach (ElementId id in CADFillPatternIds)
                    {
                        if (idsToDetele.Contains(id) == false)
                        {
                            idsToDetele.Add(id);
                        }
                    }
                    foreach (ElementId id in idsToDetele)
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted += 1;
                        }
                        catch { }
                    }
                }
                if (chkUnusedPatterns.IsChecked == true && chkCADPat.IsChecked == false)
                {
                    foreach (ElementId id in unusedFillPatternIds)
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted += 1;
                        }
                        catch { }
                    }
                }
                if (chkUnusedPatterns.IsChecked == false && chkCADPat.IsChecked == true)
                {
                    foreach (ElementId id in CADFillPatternIds)
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted += 1;
                        }
                        catch { }
                    }
                }

                t.Commit();

                Close();

                if (countDeleted == 0) { TaskDialog.Show("Result", "No fill patterns were deleted."); }
                if (countDeleted == 1) { TaskDialog.Show("Result", "1 filled pattern was deleted."); }
                if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} fill patterns were deleted."); }
            }
        }
    }  
}