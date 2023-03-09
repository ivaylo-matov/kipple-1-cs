using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeCADImportsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                var importedCADids = new List<ElementId>();
                var importedCADs = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).ToList();
                foreach (ImportInstance ii in importedCADs)
                {
                    if (ii.IsLinked == false)
                    {
                        importedCADids.Add(ii.Id);
                    }
                }

                PurgeCADImportsView window = new PurgeCADImportsView(uidoc);
                window.chkCADs.Content = $"Purge imported CAD files ({importedCADids.Count().ToString()})";
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
