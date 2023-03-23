using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeCADLinePatterns.xaml
    /// </summary>
    public partial class PurgeCADLinePatternsView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public PurgeCADLinePatternsView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge CAD Imports";
        }

        private void PurgeCADLinePatterns(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;
            using (Transaction t = new Transaction(doc, "Purge CAD Line Patterns"))
            {
                t.Start();
                if (chkCADLinePats.IsChecked == true)
                {
                    var cadLinePatternsIds = PurgeCADLinePatternsCommand.GetCADLinePatternIds(doc);
                    foreach (ElementId id in cadLinePatternsIds)
                    {
                        doc.Delete(id);
                        countDeleted += 1;
                    }
                }
                t.Commit(); 
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No line patterns were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 line pattern was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} line patterns were deleted."); }
        }
    }
}
