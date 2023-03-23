using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using View = Autodesk.Revit.DB.View;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeViewTemplatesCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Purge" + System.Environment.NewLine + "View Templates",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Purge all unused view templates in the model.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));

            splButton.AddPushButton(buttonData);
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string countTempToDelete = GetUnusedViewTemplates(doc).Count().ToString();

            PurgeViewTemplatesView window = new PurgeViewTemplatesView(uidoc);
            window.chkTemplates.Content = $"Purge unused view templates ({countTempToDelete})";
            window.ShowDialog();

            return Result.Succeeded;
        }
        public static List<ElementId> GetUnusedViewTemplates(Document doc)
        {
            var allViews = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().ToList();
            var allTemplateIds = new List<ElementId>();
            var usedTemplateIds = new List<ElementId>();
            var unusedTemplateIds = new List<ElementId>();

            foreach (View view in allViews)
            {
                if (view.IsTemplate)
                {
                    allTemplateIds.Add(view.Id);
                }
                else
                {
                    usedTemplateIds.Add(view.ViewTemplateId);
                }
            }
            foreach (ElementId id in allTemplateIds)
            {
                if (!usedTemplateIds.Contains(id))
                {
                    unusedTemplateIds.Add(id);
                }
            }
            return unusedTemplateIds;
        }
    }
}
