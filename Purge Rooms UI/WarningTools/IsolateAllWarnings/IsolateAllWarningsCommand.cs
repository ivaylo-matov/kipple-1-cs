using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class IsolateAllWarningsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<ElementId> failElIds = new List<ElementId>();

            IList<FailureMessage> allWarnings = doc.GetWarnings();
            foreach (FailureMessage warning in allWarnings)
            {
                var failSet = warning.GetFailingElements();
                foreach (ElementId id in failSet)
                {
                    if (!failElIds.Contains(id))
                    {
                        failElIds.Add(id);
                    }
                }
            }
            using (Transaction isolate = new Transaction(doc, "Isolate Warnings"))
            {
                isolate.Start();
                doc.ActiveView.IsolateElementsTemporary(failElIds);
                isolate.Commit();
            }
        return Result.Succeeded;
        }
    }
}
