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
            try
            {
                UIApplication uiApp = commandData.Application;

                var m = new BimorphModel(uiApp);
                var vm = new BimorphViewModel(m);
                var v = new BimorphView { DataContext = vm };

                v.ShowDialog();

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

