using Autodesk.Revit.UI;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System.Collections.Generic;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeViewsView.xaml
    /// </summary>
    public partial class PurgeViewsView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public PurgeViewsView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Purge Legends and Schedules";
        }

        private void PurgeUnplacedViews(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;

            using (Transaction tr = new Transaction(doc, "Purge Unused Views"))
            {
                tr.Start();

                var allPlacedViewIds = PurgeViewsCommand.GetAllPlacedViewsIds(doc);

                if (chkLegends.IsChecked == true)
                {
                    var legendIdsToDelete = PurgeViewsCommand.GetUnplacedLegendIds(doc);
                    foreach (ElementId id in legendIdsToDelete)
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted += 1;
                        }
                        catch { }
                    }
                }
                if (chkSchedules.IsChecked == true)
                {
                    var scheduleIdsToDelete = PurgeViewsCommand.GetUnplacedScheduleIds(doc);
                    foreach (ElementId id in scheduleIdsToDelete)
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted += 1;
                        }
                        catch { }
                    }
                }
                tr.Commit();
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No views were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 view was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} views were deleted."); }

        }
    }
}
