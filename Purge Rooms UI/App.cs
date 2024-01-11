using Autodesk.Revit.UI;
using Purge_Rooms_UI.DeleteSheets;
using Purge_Rooms_UI.PlaceFaceBasedFam;
using Purge_Rooms_UI.PlaceFamily;
using Purge_Rooms_UI.Bimorph;

namespace Purge_Rooms_UI
{
    class App : IExternalApplication
    {
        // define a method that will create tab, panel and buttons
        static void AddRibbonPanel(UIControlledApplication application)
        {
            string tabName = "Kipple";
 
            // create PANELS
            application.CreateRibbonTab(tabName);
            var auditPanel = application.CreateRibbonPanel(tabName, "Audit");
            var issuePanel = application.CreateRibbonPanel(tabName, "Issue");

            // create the split buttons - PURGE
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
            //PurgeSheetsCommand.CreateButton(sbPurge);
            PurgeViewsCommand.CreateButton(sbPurge);
            PurgeViewTemplatesCommand.CreateButton(sbPurge);
            PurgeViewFiltersCommand.CreateButton(sbPurge);

            // button for ARCHIVE & ISSUE
            IssueModelCommand.CreateButton(issuePanel);
            ArchiveModelCommand.CreateButton(issuePanel);
            DeleteSheetsCommand.CreateButton(issuePanel);
            BimorphCommand.CreateButton(issuePanel);
            PlaceFamilyCommand.CreateButton(issuePanel);

            // create the split buttons - WARNING
            SplitButtonData sb3Data = new SplitButtonData("warningDropdown", "Warning Tools");
            SplitButton sbWarning = auditPanel.AddItem(sb3Data) as SplitButton;
            IsolateAllWarningsCommand.CreateButton(sbWarning);
            IsolateWarningsByTypeCommand.CreateButton(sbWarning);

        }
        public Result OnShutdown(UIControlledApplication application)
        {
            // do nothing just a simple return
            return Result.Succeeded;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            // call our method that will load up our tool bar
            AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
