using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Autodesk.Revit.ApplicationServices;
using System.Linq;
using System.Text;
using System.IO;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class ArchiveModelCommand : IExternalCommand
    {
        
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
