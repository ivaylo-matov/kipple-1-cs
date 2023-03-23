using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class PurgeCADLinePatternsCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Purge CAD" + System.Environment.NewLine + "Line Patterns",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Purge all imported CAD line patterns in the model.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));

            splButton.AddPushButton(buttonData);
        }
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
