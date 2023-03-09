using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeSheetsCommand : IExternalCommand
    {
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
