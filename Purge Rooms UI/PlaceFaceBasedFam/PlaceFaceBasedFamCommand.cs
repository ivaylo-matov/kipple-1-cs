using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;

            PlaceFaceBasedFamView placeFaceBasedFam = new PlaceFaceBasedFamView(uiDoc);
            placeFaceBasedFam.Show();

            return Result.Succeeded;
        }

        /// <summary>
        /// Created a button in the Revit ribbon
        /// </summary>
        /// <param name="panel"></param>
        public static void CreateButton(RibbonPanel panel)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Place" + System.Environment.NewLine + "Family Work",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "....";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Issue.png"));

            panel.AddItem(buttonData);
        }
    }
}
