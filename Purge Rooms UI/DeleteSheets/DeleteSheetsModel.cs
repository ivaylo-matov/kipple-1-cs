using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace Purge_Rooms_UI.DeleteSheets
{
    public class DeleteSheetsModel
    {
        public UIApplication UiApp { get; }
        public Document Doc { get; }

        public DeleteSheetsModel(UIApplication uiApp)
        {
            UiApp = uiApp;
            Doc = uiApp.ActiveUIDocument.Document;
        }

        public ObservableCollection<SheetObjectWrapper> CollectionSheetObjects()
        {
            var sheetObjects = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .OrderBy(v => v.SheetNumber)
                .Select(v => new SheetObjectWrapper(v));

            return new ObservableCollection<SheetObjectWrapper>(sheetObjects);
        }

        public void Delete(List<SheetObjectWrapper> selectedSheets)
        {
            var ids = selectedSheets.Select(s => s.Id).ToList();
            using (var trans = new Transaction(Doc, "Delete Sheets"))
            {
                trans.Start();
                Doc.Delete(ids);
                trans.Commit();
            }
        }
    }
}
