using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeSheetsCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Purge Sheets",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Purge all unused sheets in the models. Those are sheets that do not contain viewports or are not assigned any revisions.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));

            splButton.AddPushButton(buttonData);
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                var unusedSheetsIds = new List<ElementId>();
                var allSheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToList();
                foreach (ViewSheet sheet in allSheets)
                {
                    if (sheet.GetAllViewports().Count() == 0 && sheet.GetAllRevisionIds().Count() == 0)
                    {
                        unusedSheetsIds.Add(sheet.Id);
                    }
                }

                string countUnusedSheets = unusedSheetsIds.Count().ToString();
                PurgeSheetsView window = new PurgeSheetsView(uidoc);
                window.chkPurgeSheets.Content = $"Purge all unused sheets ({countUnusedSheets})";
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;            
            }  
        }
    }
}
