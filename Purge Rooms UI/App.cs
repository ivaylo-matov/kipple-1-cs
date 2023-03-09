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

            // create the push buttons below PurgeRooms
            PushButtonData purgeRoomsData = new PushButtonData("cmdPurgerooms",
                "Purge Rooms",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeRoomsCommand");
            purgeRoomsData.ToolTip = "Purge all not placed or/and not enclosed rooms in the model.";
            purgeRoomsData.LargeImage = purgeIcon;
            // button for Purge Areas
            PushButtonData purgeAreaData = new PushButtonData("cmdPurgeAreas",
                "Purge Areas",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeAreasCommand");
            purgeAreaData.ToolTip = "Purge all not placed or/and not enclosed areas in the model.";
            purgeAreaData.LargeImage = purgeIcon;
            // button for Purge Fill Patterns
            PushButtonData purgeFillPatData = new PushButtonData("cmdPurgefrs",
                "Purge Fill" + System.Environment.NewLine + "Patterns",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeFillPatternsCommand");
            purgeFillPatData.ToolTip = "Purge all unused or/and imported CAD fill patterns in the model.";
            purgeFillPatData.LargeImage = purgeIcon;
            // button for Purge CAD Imports
            PushButtonData purgeImportData = new PushButtonData("cmdPurgeCADs",
                "Purge CAD" + System.Environment.NewLine + "Imports",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeCADImportsCommand");
            purgeImportData.ToolTip = "Purge all imported CAD files in the model.";
            purgeImportData.LargeImage = purgeIcon;
            // button for Purge CAD Line Patterns
            PushButtonData purgeLinePatData = new PushButtonData("cmdPurgeCADlineptas",
                "Purge CAD" + System.Environment.NewLine + "Line Patterns",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeCADLinePatternsCommand");
            purgeLinePatData.ToolTip = "Purge all imported CAD line patterns in the model.";
            purgeLinePatData.LargeImage = purgeIcon;
            // button for Purge Sheets
            PushButtonData purgeSheetData = new PushButtonData("cmdPurgeSheets",
                "Purge Sheets",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeSheetsCommand");
            purgeSheetData.ToolTip = "Purge all unused seets in the models. Those are sheets that do not contain viewports or are not assigned any revisions.";
            purgeSheetData.LargeImage = purgeIcon;
            // button for Purge Legends and Schedules
            PushButtonData purgeViewData = new PushButtonData("cmdPurgeViews",
                "Purge Legends" + System.Environment.NewLine + "+ Schedules",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeViewsCommand");
            purgeViewData.ToolTip = "Purge all unplaced legend and/or schedule views in the model.";
            purgeViewData.LargeImage = purgeIcon;
            // button for Purge Views Templates
            PushButtonData purgeTemplateData = new PushButtonData("cmdPurgeViewTemplates",
                "Purge View" + System.Environment.NewLine + "Templates",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeViewTemplatesCommand");
            purgeTemplateData.ToolTip = "Purge all unused view templates in the model.";
            purgeTemplateData.LargeImage = purgeIcon;
            // button for Purge Views Filters
            PushButtonData purgeFilterData = new PushButtonData("cmdPurgeViewFilters",
                "Purge View" + System.Environment.NewLine + "Filters",
                thisAssemblyPath,
                "Purge_Rooms_UI.PurgeViewFiltersCommand");
            purgeFilterData.ToolTip = "Purge all unused view filters in the model.";
            purgeFilterData.LargeImage = purgeIcon;


            // create the split bttons - PURGE
            SplitButtonData sb1Data = new SplitButtonData("purgeDropdown", "Purge Tools");
            SplitButton sbPurge = auditPanel.AddItem(sb1Data) as SplitButton;
            sbPurge.AddPushButton(purgeImportData);
            sbPurge.AddSeparator();
            sbPurge.AddPushButton(purgeRoomsData);
            sbPurge.AddPushButton(purgeAreaData);
            sbPurge.AddSeparator();
            sbPurge.AddPushButton(purgeFillPatData);
            sbPurge.AddPushButton(purgeLinePatData);
            sbPurge.AddSeparator();                       
            sbPurge.AddPushButton(purgeSheetData);
            sbPurge.AddPushButton(purgeViewData);
            sbPurge.AddPushButton(purgeTemplateData);
            sbPurge.AddPushButton(purgeFilterData);


            // create the push buttons below CHECK
            PushButtonData check1Data = new PushButtonData("cmdCheckFams", "Check Family" + System.Environment.NewLine + "Naming", thisAssemblyPath,"Purge_Rooms_UI.CheckFamilyNaming");
            check1Data.ToolTip = "Check for any familien not named in copliance with the ACG protocols.";
            check1Data.LargeImage = checkIcon;

            // create the split bttons - CHECK
            SplitButtonData sb2Data = new SplitButtonData("checkDropdown", "Check Tools");
            SplitButton sbCheck = auditPanel.AddItem(sb2Data) as SplitButton;
            sbCheck.AddPushButton(check1Data);

            // button for ARCHIVE & ISSUE
            PushButtonData issueData = new PushButtonData("cmdIssue", "Issue" + System.Environment.NewLine + "Model", thisAssemblyPath, "Purge_Rooms_UI.IssueModelCommand");
            issueData.ToolTip = "Prepare the model for issue." + System.Environment.NewLine + "Please make sure all users have synced before funning the tool." +
                "This tool works on workshared cloud models. The clean model will be saved in the project's 01 WIP - Internal Work folder.";
            issueData.LargeImage = issueIcon;
            PushButton issueButton = issuePanel.AddItem(issueData) as PushButton;
            PushButtonData archiveData = new PushButtonData("cmdArchive", "Archive" + System.Environment.NewLine + "Model", thisAssemblyPath, "Purge_Rooms_UI.ArchiveModelCommand");
            archiveData.ToolTip = "Archive the model." + System.Environment.NewLine + "Please sync the model before running the tool." +
                "This tool works on workshared cloud models." +
                "All Project Information parameter must be filled in correctly.";
            archiveData.LargeImage = archiveIcon;
            PushButton archiveButton = issuePanel.AddItem(archiveData) as PushButton;

            // JUST A TEST
            //PushButtonData nwcData = new PushButtonData("cmdNWC", "NWC Export", thisAssemblyPath, "Purge_Rooms_UI.Schedules");
            //archiveData.ToolTip = "Archive the model." + System.Environment.NewLine + "Please sync the model before running the tool." +
            //    "This tool works on workshared cloud models." +
            //    "All Project Information parameter must be filled in correctly.";
            //archiveData.LargeImage = archiveIcon;
            //PushButton nwcButton = issuePanel.AddItem(nwcData) as PushButton;

        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // do nothing just a simple return
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // call our method that will load up our toolbar
            application.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.TestProcessor);

            AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
