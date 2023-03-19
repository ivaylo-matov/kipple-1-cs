using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Purge_Rooms_UI
{
    class App : IExternalApplication
    {
        // define a method that will create our tab and button
        static void AddRibbonPanel(UIControlledApplication application)
        {
            string tabName = "Kipple";
 
            // create PANELS
            application.CreateRibbonTab(tabName);
            var auditPanel = application.CreateRibbonPanel(tabName, "Audit");
            var issuePanel = application.CreateRibbonPanel(tabName, "Issue");

            // Get dll assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // create IMAGES for the buttons
            BitmapImage purgeIcon = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Purge.png"));
            BitmapImage checkIcon = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Check.png"));
            BitmapImage archiveIcon = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Archive.png"));
            BitmapImage issueIcon = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Issue.png"));
            BitmapImage warningIcon = new BitmapImage(new Uri("pack://application:,,,/Purge Rooms UI;component/Resources/Warning.png"));

            // create the split bttons - PURGE
            SplitButtonData sb1Data = new SplitButtonData("purgeDropdown", "Purge Tools");
            SplitButton sbPurge = auditPanel.AddItem(sb1Data) as SplitButton;
            PurgeCADImportsCommand.CreateButton(sbPurge);
            sbPurge.AddSeparator();
            PurgeRoomsCommand.CreateButton(sbPurge);
            PurgeAreasCommand.CreateButton(sbPurge);
            sbPurge.AddSeparator();
            PurgeFillPatternsCommand.CreateButton(sbPurge);
            PurgeCADLinePatternsCommand.CreateButton(sbPurge);
            sbPurge.AddSeparator();
            PurgeSheetsCommand.CreateButton(sbPurge);
            PurgeViewsCommand.CreateButton(sbPurge);
            PurgeViewTemplatesCommand.CreateButton(sbPurge);
            PurgeViewFiltersCommand.CreateButton(sbPurge);

            // button for ARCHIVE & ISSUE
            IssueModelCommand.CreateButton(issuePanel);
            ArchiveModelCommand.CreateButton(issuePanel);
            IsolateWarningsByTypeCommand.CreateButton(issuePanel);


            //// create the push buttons below CHECK
            //PushButtonData check1Data = new PushButtonData("cmdCheckFams", "Check Family" + System.Environment.NewLine + "Naming", thisAssemblyPath,"Purge_Rooms_UI.CheckFamilyNaming");
            //check1Data.ToolTip = "Check for any familien not named in copliance with the ACG protocols.";
            //check1Data.LargeImage = checkIcon;

            //// create the split bttons - CHECK
            //SplitButtonData sb2Data = new SplitButtonData("checkDropdown", "Check Tools");
            //SplitButton sbCheck = auditPanel.AddItem(sb2Data) as SplitButton;
            //sbCheck.AddPushButton(check1Data);




            // JUST A TEST
            PushButtonData allWarnData = new PushButtonData("cmdNWC", "Isolate All", thisAssemblyPath, "Purge_Rooms_UI.IsolateAllWarningsCommand");
            allWarnData.ToolTip = "Isolate all elements visible in the active view that have warnings associated to them";
            allWarnData.LargeImage = warningIcon;
            PushButtonData typeWarnData = new PushButtonData("warnByType", "Isolate" + System.Environment.NewLine + "By Type", thisAssemblyPath, "Purge_Rooms_UI.IsolateWarningsByTypeCommand");
            typeWarnData.ToolTip = "Isolate all elements visible in the active view that associated to a selected warning type.";
            typeWarnData.LargeImage = warningIcon;


            // create the split bttons - WARNING
            SplitButtonData sb3Data = new SplitButtonData("warningDropdown", "Warning Tools");
            SplitButton sbWarning = auditPanel.AddItem(sb3Data) as SplitButton;
            sbWarning.AddPushButton(allWarnData);
            sbWarning.AddPushButton(typeWarnData);

        }
        public Result OnShutdown(UIControlledApplication application)
        {
            // do nothing just a simple return
            return Result.Succeeded;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            // call our method that will load up our toolbar
            //application.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);

            AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
