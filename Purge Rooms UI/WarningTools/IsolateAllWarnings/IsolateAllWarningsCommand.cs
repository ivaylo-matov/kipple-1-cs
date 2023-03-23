using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class IsolateAllWarningsCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Isolate" + System.Environment.NewLine + "All Warnings",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Isolate all elements visible in the active view that have warnings associated to them.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Warning.png"));

            splButton.AddPushButton(buttonData);
        }
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
