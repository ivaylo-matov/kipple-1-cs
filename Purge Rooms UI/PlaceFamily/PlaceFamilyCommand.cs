using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Purge_Rooms_UI.PlaceFaceBasedFam;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI.PlaceFamily
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class PlaceFamilyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                PlaceFamilyModel model = new PlaceFamilyModel
                {
                    UiDoc = uiDoc
                };

                PlaceFamilyViewModel viewModel = new PlaceFamilyViewModel(model);
                PlaceFamilyView view = new PlaceFamilyView(model);
                view.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error: {ex.Message}");
                return Result.Failed;
            }
        }

        public static void CreateButton(RibbonPanel panel)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Place" + Environment.NewLine + "Family",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );

            buttonData.ToolTip = "Place Family on Wall Face";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Issue.png"));

            panel.AddItem(buttonData);
        }
    }
}
