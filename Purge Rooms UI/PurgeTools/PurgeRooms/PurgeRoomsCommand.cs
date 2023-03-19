using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeRoomsCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Purge Rooms",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Purge all rooms not placed or/and not enclosed in the model.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));

            splButton.AddPushButton(buttonData);
        }
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
