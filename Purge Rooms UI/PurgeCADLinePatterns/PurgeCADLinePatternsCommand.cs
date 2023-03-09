using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeCADLinePatternsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                PurgeCADLinePatternsView window = new PurgeCADLinePatternsView(uidoc);
                window.chkCADLinePats.Content = $"Purge all imported CAD line patterns ({GetCADLinePatternIds(doc).Count().ToString()})";
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
        public static List<ElementId> GetCADLinePatternIds(Document doc)
        {
            List<ElementId> cadLinePatternsIds = new List<ElementId>();
            try
            {
                var allLinePatterns = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement)).ToList();
                foreach (LinePatternElement pat in allLinePatterns)
                {
                    if (pat.Name.Contains("IMPORT") && cadLinePatternsIds.Contains(pat.Id) == false)
                    {
                        cadLinePatternsIds.Add(pat.Id);
                    }
                }
            }
            catch { }
            return cadLinePatternsIds;
        }
    }
}
