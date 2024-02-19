using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class IssueCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                Application app = uiapp.Application;

                IssueModel model = new IssueModel(uiapp);
                IssueViewModel viewModel = new IssueViewModel(model);
                IssueView view = new IssueView { DataContext = viewModel };

                view.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex) { return Result.Failed; }
        }

        public static void CreateButton(RibbonPanel panel)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData(
                MethodBase.GetCurrentMethod().DeclaringType?.Name,
                "Issue" + System.Environment.NewLine + "Model",
                thisAssemblyPath,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName
                );
            buttonData.ToolTip = "Prepare the model for issue." + System.Environment.NewLine +
                "Please make sure all users have synced before funning the tool." + System.Environment.NewLine +
                "This tool works on workshared cloud models. The clean model will be saved in the project's 01 WIP - Internal Work folder.";
            buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Issue.png"));

            panel.AddItem(buttonData);
        }
    }
}
