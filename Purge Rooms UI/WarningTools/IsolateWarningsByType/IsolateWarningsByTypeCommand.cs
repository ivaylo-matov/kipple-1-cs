using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class IsolateWarningsByTypeCommand : IExternalCommand
    {
        public static void CreateButton(SplitButton splButton)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Isolate" + System.Environment.NewLine + "By Type",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Isolate all elements visible in the active view that associated to a selected warning type.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Warning.png"));

            splButton.AddPushButton(buttonData);
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
   
            Isolatetest window = new Isolatetest(uidoc);
            window.ShowDialog();
            return Result.Succeeded;
        }
        public static IList<string> GetInput(Document doc)
        {
            IList<FailureMessage> warnings = doc.GetWarnings();
            IList<string> unqDescriptions = new List<string>();
            
            foreach (FailureMessage warning in warnings)
            {
                string fullDescription = warning.GetDescriptionText();
                string inputDescription = fullDescription.Split('.')[0];
                if (!unqDescriptions.Contains(inputDescription))
                {
                    unqDescriptions.Add(inputDescription);
                }
            }
            
            IList<string> descriptionInput = new List<string>();
            IList<int> descriptionCounts = new List<int>();
            foreach (string description in unqDescriptions)
            {
                int count = 0;
                foreach (FailureMessage fail in warnings)
                {
                    if (fail.GetDescriptionText().Contains(description))
                    {
                        count++;
                    }
                }
                descriptionInput.Add($"{count} : {description}");
                descriptionCounts.Add(count);
            }

            IList<string> sortedDescriptions = descriptionInput.OrderBy(x => descriptionCounts[descriptionInput.IndexOf(x)]).Reverse().ToList();
            return sortedDescriptions;
        }
    }
}
