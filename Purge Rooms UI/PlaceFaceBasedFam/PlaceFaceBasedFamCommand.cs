using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI.PlaceFaceBasedFam
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceFaceBasedFamCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uiDoc.Document;

            PlaceFaceBasedFamView placeFaceBasedFam = new PlaceFaceBasedFamView(uiDoc);
            placeFaceBasedFam.Show();

            return Result.Succeeded;
        }

        public static void CreateButton(RibbonPanel panel)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Place" + System.Environment.NewLine + "Family",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "....";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Issue.png"));

            panel.AddItem(buttonData);
        }

        public class PlaceFamilyExternalEventHandler : IExternalEventHandler
        {
            public void Execute(UIApplication app)
            {
                // Your transaction code goes here
                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc.Document;

                // Your existing transaction code
                Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Face, "Select a wall face");
                // Rest of your transaction code...
            }

            public string GetName()
            {
                return "PlaceFamilyExternalEventHandler";
            }
        }
    }
}
