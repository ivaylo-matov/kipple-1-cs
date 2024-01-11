using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Purge_Rooms_UI.Bimorph
{
    [Transaction(TransactionMode.Manual)]
    internal class BimorphCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            BimorphView placeFaceBasedFam = new BimorphView(uiDoc);
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
                "Bimorph",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "....";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Issue.png"));

            panel.AddItem(buttonData);
        }
    }
}

