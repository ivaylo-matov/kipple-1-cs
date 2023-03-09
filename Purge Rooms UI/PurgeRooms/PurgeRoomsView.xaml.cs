using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace Purge_Rooms_UI
{    
    public partial class PurgeRoomsView : Window
    {
        public UIDocument uidoc { get;}

        public Document doc { get; }

        public PurgeRoomsView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge Rooms";
        }

        private void Purge(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;

            using (Transaction t = new Transaction(doc, "Purge rooms"))
            {
                t.Start();
                if (chkPlaced.IsChecked == true)
                {
                    var unplaced = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .OfType<Room>()
                        .Where(r => r.Area <= 0)
                        .Where(r => r.Location == null)
                        .ToList();
                    foreach (Room r in unplaced)
                    {
                        doc.Delete(r.Id);
                        countDeleted += 1;
                    }
                }
                if (chkEnclosed.IsChecked == true)
                {
                    var notEnclosed = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .OfType<Room>()
                        .Where(r => r.Area <= 0)
                        .Where(r => r.Location != null)
                        .ToList();                 
                    foreach (Room r in notEnclosed)
                    {
                        doc.Delete(r.Id);
                        countDeleted += 1;
                    }          
                }

                t.Commit();
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No rooms were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 room was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} rooms were deleted."); }

        }


    }
}
