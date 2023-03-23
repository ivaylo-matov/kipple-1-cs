using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Autodesk.Revit.ApplicationServices;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class ArchiveModelCommand : IExternalCommand
    {
        public static void CreateButton(RibbonPanel panel)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Archive" + System.Environment.NewLine + "Model",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Archive the model." + System.Environment.NewLine + "Please sync the model before running the tool. " +
                "This tool works on workshared cloud models. " +
                "All Project Information parameter must be filled in correctly.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Archive.png"));

            panel.AddItem(buttonData);
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            
            var dirParam = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_ProjectInformation).ToElements()[0];
            string dirPathValue = dirParam.LookupParameter("Project Directory").AsString();

            if (dirPathValue.EndsWith("Sharing") && doc.IsModelInCloud == true)
            {
                DateTime today = DateTime.Today;
                string date = today.ToString("yyMMdd");
                string dirPath = $"{dirPathValue}\\04 Archived\\{date}";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                // assemble the new archived model name and model path
                string currentName = doc.Title;
                string modelPath = $"{dirPath}\\ARCHIVED_{date}_{currentName}.rvt";
                try
                {
                    WorksharingSaveAsOptions wsOpt = new WorksharingSaveAsOptions();
                    wsOpt.SaveAsCentral = true;
                    SaveAsOptions saveOpt = new SaveAsOptions();
                    saveOpt.SetWorksharingOptions(wsOpt);
                    saveOpt.OverwriteExistingFile = true;
                    saveOpt.Compact = false;
                    saveOpt.MaximumBackups = 1;
                    doc.SaveAs(modelPath, saveOpt);
                    TaskDialog.Show("", $"Archive model saved in {dirPath}.");
                }
                catch(Exception ex)
                {
                    message = ex.Message;
                }
            }
            else
            {
                TaskDialog.Show("", "Project directory not valid.");
            }
            return Result.Succeeded;
        }
    }
}
