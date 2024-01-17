using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI.Bimorph
{
    [Transaction(TransactionMode.Manual)]
    internal class BimorphCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;  // get the UiApp

                var m = new BimorphModel(uiApp);                // create instance of model and pass it UiApp
                var vm = new BimorphViewModel(m);               // create instance of view-model and pass it model
                var v = new BimorphView { DataContext = vm };   // create instance of view and link it to view-model

                v.ShowDialog();                                 // show the UI

                return Result.Succeeded;
            }
            catch (Exception ex) { return Result.Failed; }            
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

