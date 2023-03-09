using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeAreasCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string countUplaced = GetUnplacedAreaIds(doc).Count().ToString();
            string countUnenclosed = GetUnenclosedAreaIds(doc).Count().ToString();

            PurgeAreasView window = new PurgeAreasView(uidoc);
            window.chkPlaced.Content = $"Purge unplaced areas ({countUplaced})";
            window.chkEnclosed.Content = $"Purge not enclosed areas ({countUnenclosed})";
            window.ShowDialog();

            return Result.Succeeded;
        }
        public static List<ElementId> GetUnplacedAreaIds(Document doc)
        {
            List<ElementId> unplacedAreaIds = new List<ElementId>();
            try
            {
                var unplacedAreas = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .OfType<Area>()
                        .Where(r => r.Area <= 0)
                        .Where(r => r.Location == null)
                        .ToList();
                foreach (Area a in unplacedAreas)
                {
                    unplacedAreaIds.Add(a.Id);
                }
            }
            catch { }
            return unplacedAreaIds;
        }
        public static List<ElementId> GetUnenclosedAreaIds(Document doc)
        {
            List<ElementId> unenclosedAreaIds = new List<ElementId>();
            try
            {
                var unenclosedAreas = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .OfType<Area>()
                        .Where(r => r.Area <= 0)
                        .Where(r => r.Location != null)
                        .ToList();
                foreach (Area a in unenclosedAreas)
                {
                    unenclosedAreaIds.Add(a.Id);
                }
            }
            catch { }
            return unenclosedAreaIds;
        }
    }
}
