using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeViewsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string countUnplacedLegends = GetUnplacedLegendIds(doc).Count().ToString();
            string countUnplacedSchedules = GetUnplacedScheduleIds(doc).Count().ToString();

            PurgeViewsView window = new PurgeViewsView(uidoc);
            window.chkLegends.Content = $"Purge all legend views not placed on sheets ({countUnplacedLegends})";
            window.chkSchedules.Content = $"Purge all schedule views not placed on sheets ({countUnplacedSchedules})";
            window.ShowDialog();

            return Result.Succeeded;
        }

        public static List<ElementId> GetAllPlacedViewsIds(Document doc)
        {
            List<ElementId> allPlacedViewsIds = new List<ElementId>();
            List<ViewSheet> allSheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().Cast<ViewSheet>().ToList();
            foreach (ViewSheet sheet in allSheets)
            {
                var viewsOnThisSheet = sheet.GetAllPlacedViews();
                foreach (ElementId viewId in viewsOnThisSheet)
                {
                    if (allPlacedViewsIds.Contains(viewId) == false)
                    {
                        allPlacedViewsIds.Add(viewId);
                    }
                }
            }
            return allPlacedViewsIds;
        }
        public static List<ElementId> GetUnplacedLegendIds(Document doc)
        {
            List<ElementId> unplacedLegendIds = new List<ElementId>();
            List<ElementId> allPlacedViewIds = GetAllPlacedViewsIds(doc);

            List<View> allLegends = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().Cast<View>().Where(v => v.ViewType == ViewType.Legend).ToList();
            foreach (View leg in allLegends)
            {
                if (allPlacedViewIds.Contains(leg.Id) == false && unplacedLegendIds.Contains(leg.Id) == false)
                {
                    unplacedLegendIds.Add(leg.Id);
                }
            }
            return unplacedLegendIds;
        }
        public static List<ElementId> GetUnplacedScheduleIds(Document doc)
        {
            var schedulesOnSheets = new FilteredElementCollector(doc).OfClass(typeof(ScheduleSheetInstance)).ToElements().OfType<ScheduleSheetInstance>().Where(v => !v.Name.Contains("Revision Schedule")).ToList();
            var allSchedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule)).Cast<ViewSchedule>().Where(v => !v.Name.Contains("Revision Schedule")).ToList();

            List<string> schOnSheetNames = new List<string>();
            foreach (var schOnSheet in schedulesOnSheets)
            {
                if (!schOnSheetNames.Contains(schOnSheet.Name))
                {
                    schOnSheetNames.Add(schOnSheet.Name);
                }
            }
            List<ElementId> schNotOnSheets = new List<ElementId>();
            foreach (var sch in allSchedules)
            {
                if (!schOnSheetNames.Contains(sch.Name))
                {
                    schNotOnSheets.Add(sch.Id);
                }
            }
            return schNotOnSheets;
        }    
    }
}
