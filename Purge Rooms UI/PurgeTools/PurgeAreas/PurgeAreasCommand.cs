using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeAreasCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Purge Areas",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Purge all not placed or/and not enclosed areas in the model.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));

            splButton.AddPushButton(buttonData);
        }
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
