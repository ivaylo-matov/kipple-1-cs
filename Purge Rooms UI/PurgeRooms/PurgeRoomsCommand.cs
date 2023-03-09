using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeRoomsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                var unplaced = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .OfType<Room>()
                        .Where(r => r.Area <= 0)
                        .Where(r => r.Location == null)
                        .ToList();
                var notEnclosed = new FilteredElementCollector(doc)
                        .OfClass(typeof(SpatialElement))
                        .OfType<Room>()
                        .Where(r => r.Area <= 0)
                        .Where(r => r.Location != null)
                        .ToList();

                string countUplaced = unplaced.Count().ToString();
                string countNotEnclosed = notEnclosed.Count().ToString();



                PurgeRoomsView window = new PurgeRoomsView(uidoc);
                window.chkPlaced.Content = $"Purge unplaced rooms ({countUplaced})";
                window.chkEnclosed.Content = $"Purge not enclosed rooms ({countNotEnclosed})";
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception e) 
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
