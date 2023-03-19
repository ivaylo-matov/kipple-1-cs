using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeViewFiltersCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Purge" + System.Environment.NewLine + "View Filters",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Purge all unused view filters in the model.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));

            splButton.AddPushButton(buttonData);
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string countFiltersToDelete = GetUnusedViewFilters(doc).Count().ToString();

            PurgeViewFiltersView window = new PurgeViewFiltersView(uidoc);
            window.chkFilters.Content = $"Purge unused view filters ({countFiltersToDelete})";
            window.ShowDialog();

            return Result.Succeeded;
        }
        public static List<ElementId> GetUnusedViewFilters(Document doc)
        {
            var allViews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().ToList();
            List<ParameterFilterElement> allFilters = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).WhereElementIsNotElementType().Cast<ParameterFilterElement>().ToList();
            List<ElementId> unusedFilterIds = new List<ElementId>();
            List<ElementId> usedFilterIds = new List<ElementId>();

            foreach (View view in allViews)
            {
                try
                {
                    var viewFilterIds = view.GetFilters();
                    foreach (ElementId id in viewFilterIds)
                    {
                        if (!usedFilterIds.Contains(id))
                        {
                            usedFilterIds.Add(id);
                        }
                    }
                }
                catch { }
            }
            foreach (ParameterFilterElement filter in allFilters)
            {
                if (!usedFilterIds.Contains(filter.Id))
                {
                    unusedFilterIds.Add(filter.Id);
                }
            }
            return unusedFilterIds;
        }
    }
}
