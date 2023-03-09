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
    public class PurgeViewFiltersCommand : IExternalCommand
    {
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
