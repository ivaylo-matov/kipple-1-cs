using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UIFramework;
using WinForms = System.Windows.Forms;

namespace Purge_Rooms_UI
{
    public class IssueModel
    {
        public UIApplication UIApp { get; }
        public Document Doc { get; }
        public IssueModel(UIApplication uiApp)
        {
            UIApp = uiApp;
            Doc = uiApp.ActiveUIDocument.Document;
        }

        // HOLD ELEMENTS HERE
        private static List<RevitLinkType> LogRVTLinks = new List<RevitLinkType>();
        private static List<ImportInstance> LogCADLinks = new List<ImportInstance>();
        private static List<ImageType> LogIMGLinks = new List<ImageType>();
        private static List<View> LogViews = new List<View>();
        private static List<Phase> LogLibPhase = new List<Phase>();
        private static List<Group> LogModelGroups = new List<Group>();

        public static string TargetFolderNotFoundMessage = "Not found! Please navigate to target folder.";


        public void Run()
        {

        }
        // Pre-process elements
        public bool EnableRVTLinks()
        {
            LogRVTLinks = new FilteredElementCollector(Doc)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .ToList();
            return LogRVTLinks.Count > 0;
        }
        public bool EnableCADLinks()
        {
            LogCADLinks = new FilteredElementCollector(Doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .ToList();
            return LogCADLinks.Count > 0;
        }
        public bool EnableIMGLinks()
        {
            LogIMGLinks = new FilteredElementCollector(Doc)
                .OfClass(typeof(ImageType))
                .Cast<ImageType>()
                .ToList();
            return LogIMGLinks.Count > 0;
        }
        public bool EnableViews()
        {
            LogViews = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.Name != "IFC Export")
                .Where(v => v.Name != "Navisworks")
                .Where(v => v.Id != Doc.ActiveView.Id)
                .ToList();
            return LogViews.Count > 0;
        }
        public bool EnableLibPhase()
        {
            LogLibPhase = new FilteredElementCollector(Doc)
                .OfClass(typeof(Phase))
                .Cast<Phase>()
                .Where(p => p.Name.Contains("Library"))
                .ToList();
            return LogLibPhase.Count > 0;
        }
        public bool EnableGroups()
        {
            LogModelGroups = new FilteredElementCollector(Doc)
                .OfClass(typeof(Group))
                .Cast<Group>()
            .ToList();
            return LogModelGroups.Count > 0;
        }

        public void RemoveRVTLinks()
        {
            using (Transaction tRVT = new Transaction(Doc, "Remove RVT links"))
            {
                tRVT.Start();
                foreach (var i in LogRVTLinks)
                {
                    try { Doc.Delete(i.Id); }
                    catch { }
                }
                tRVT.Commit();
            }                
        }
        public void RemoveCADLinks()
        {
            using (Transaction tCAD = new Transaction(Doc, "Remove CAD links"))
            {
                tCAD.Start();
                foreach (var i in LogCADLinks)
                {
                    try { Doc.Delete(i.Id); }
                    catch { }
                }
                tCAD.Commit();
            }            
        }
        public void RemoveIMGLinks()
        {
            using (Transaction tIMG = new Transaction(Doc, "Remove Image & PDF links"))
            {
                tIMG.Start();
                foreach (var i in LogIMGLinks)
                {
                    try { Doc.Delete(i.Id); }
                    catch { }
                }
                tIMG.Commit();
            }
        }
        public void RemoveViews()
        {
            using (Transaction tViews = new Transaction(Doc, "Remove Sheets & Views"))
            {
                tViews.Start();
                foreach (var i in LogViews)
                {
                    try { Doc.Delete(i.Id); }
                    catch { }
                }
                tViews.Commit();
            }
        }
        public void RemoveLibPhaseElements()
        {
            using (Transaction tLib = new Transaction(Doc, "Remove Elements from Library phase"))
            {
                tLib.Start();

                // delete the groups first - collect all model groups
                var allGroups = new FilteredElementCollector(Doc)
                .OfClass(typeof(Group))
                .WhereElementIsNotElementType()
                .Where(e => e.get_Parameter(BuiltInParameter.PHASE_CREATED) != null)
                .Where(e => e.get_Parameter(BuiltInParameter.PHASE_DEMOLISHED) != null)
                .ToList();

                List<ElementId> libIdsToDelete = new List<ElementId>();
                foreach (var el in allGroups)
                {
                    if (el.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString().Contains("Library") && el.get_Parameter(BuiltInParameter.PHASE_DEMOLISHED).AsValueString().Contains("Existing"))
                    { libIdsToDelete.Add(el.Id); }
                }
                foreach (ElementId id in libIdsToDelete)
                {
                    try { Doc.Delete(id); }
                    catch { }
                }

                // delete element that are not in groups
                var allElements = new FilteredElementCollector(Doc)
                    .WhereElementIsNotElementType()
                    .Where(e => e.get_Parameter(BuiltInParameter.PHASE_CREATED) != null)
                    .Where(e => e.get_Parameter(BuiltInParameter.PHASE_DEMOLISHED) != null)
                    .ToList();

                libIdsToDelete = new List<ElementId>();
                foreach (var el in allElements)
                {
                    if (el.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString().Contains("Library") && el.get_Parameter(BuiltInParameter.PHASE_DEMOLISHED).AsValueString().Contains("Existing"))
                    { libIdsToDelete.Add(el.Id); }
                }
                foreach (ElementId id in libIdsToDelete)
                {
                    try { Doc.Delete(id); }
                    catch { }
                }

                tLib.Commit();
            }
                
        }
        public void UngroupGroups()
        {
            using (Transaction tGroup = new Transaction(Doc, "Ungroup all model groups"))
            {
                tGroup.Start();

                // delete host (not nested) groups first
                foreach (var group in LogModelGroups)
                {
                    try
                    {
                        if (group.AttachedParentId == null) { group.UngroupMembers(); }
                    }
                    catch { }
                }
                // delete nested groups
                var nestedGroups = new FilteredElementCollector(Doc)
                    .OfClass(typeof(Group))
                    .Cast<Group>().ToList();
                foreach (Group group in nestedGroups)
                {
                    try { group.UngroupMembers(); }
                    catch { }
                }

                tGroup.Commit();
            }
        }
        public void RemoveNonCoordViews()
        {
            // delete sheets
            var nonCoordSheets = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(s => !s.SheetNumber.Contains("Project Information"))
                .Where(s => !s.LookupParameter("Sheet Phase").AsString().Contains("COORD"))
                .ToList();

            foreach (ViewSheet sheet in nonCoordSheets)
            {
                try
                { Doc.Delete(sheet.Id); }
                catch { }
            }

            // delete views
            var nonCoordViews = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.Name != "IFC Export")
                .Where(v => v.Name != "Navisworks")
                .ToList();

            foreach (View view in nonCoordViews)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue) continue;
                else
                {
                    if (view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        try
                        {
                            Doc.Delete(view.Id);
                        }
                        catch { }
                    }
                }
            }

            // delete schedules
            var allSchedules = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Schedules)
                .WhereElementIsNotElementType()
                .Cast<View>().ToList();

            foreach (View sch in allSchedules)
            {
                if (!sch.LookupParameter("View Folder 1 (View type)").HasValue) continue;
                else
                {
                    if (sch.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        try
                        {
                            Doc.Delete(sch.Id);
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Looks at the Splash Page, which should be the active view and returns the data from the latest revision.
        /// It also looks for the project directory in Project Information.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CollectCurrentMetaData()
        {
            ViewSheet splashScr = Doc.ActiveView as ViewSheet;
            Parameter approvedByParam = splashScr.get_Parameter(BuiltInParameter.SHEET_APPROVED_BY);            

            ProjectInfo info = new FilteredElementCollector(Doc)
                .OfClass(typeof(ProjectInfo))
                .Cast<ProjectInfo>()
                .FirstOrDefault();

            string targetDir = TargetFolderNotFoundMessage;  

            if (info.LookupParameter("Project Directory") != null)
            {
                string projectDir = info.LookupParameter("Project Directory").AsValueString();
                targetDir = $"{projectDir}\\01 WIP - Internal Work\\{DateTime.Now.ToString("yyMMdd")}";
            }

            ICollection<ElementId> currentRevs = splashScr.GetAdditionalRevisionIds();
            // try to get the data from the latest revision
            if (currentRevs.Count() > 0)
            {
                Revision lastRev = Doc.GetElement(currentRevs.Last()) as Revision;

                return new Dictionary<string, string> {
                { "IssuedTo", lastRev.IssuedTo },
                { "IssuedBy", lastRev.IssuedBy },
                { "ApprovedBy", approvedByParam.AsValueString() },
                { "RevDescription", lastRev.Description },
                { "TargetDir", targetDir},
                { "TargetFileName", Doc.Title}
                };
            }
            else
            {
                return new Dictionary<string, string> {
                { "IssuedTo", "" },
                { "IssuedBy", GetUserInitials(UIApp) },
                { "ApprovedBy", approvedByParam.AsValueString() },
                { "RevDescription", "" },
                { "TargetDir", targetDir},
                { "TargetFileName", Doc.Title}
                };
            }
        }

        /// <summary>
        /// Pushed the revised metadata to the Splash Page, which should be the active view.
        /// </summary>
        /// <param name="revDescription"></param>
        /// <param name="issuedBy"></param>
        /// <param name="issuedTo"></param>
        /// <param name="approvedBy"></param>
        public void UpdateMetaData(string revDescription, string issuedBy, string issuedTo, string approvedBy)
        {
            using (Transaction tMeta = new Transaction(Doc, "Update metadata"))
            {
                tMeta.Start();
                try
                {
                    ViewSheet splashScr = Doc.ActiveView as ViewSheet;
                    Parameter approvedByParam = splashScr.get_Parameter(BuiltInParameter.SHEET_APPROVED_BY);
                    ICollection<ElementId> currentRevs = splashScr.GetAdditionalRevisionIds();

                    Revision newRev = Revision.Create(Doc);
                    newRev.Description = revDescription;
                    newRev.IssuedBy = issuedBy;
                    newRev.IssuedTo = issuedTo;
                    newRev.RevisionDate = DateTime.Now.ToString("dd.MM.yy");

                    currentRevs.Add(newRev.Id);
                    splashScr.SetAdditionalRevisionIds(currentRevs);
                    approvedByParam.Set(approvedBy);
                    splashScr.get_Parameter(BuiltInParameter.SHEET_DRAWN_BY).Set(issuedBy);
                    splashScr.get_Parameter(BuiltInParameter.SHEET_ISSUE_DATE).Set(DateTime.Now.ToString("dd.MM.yy"));
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error!", $"Metadata could not be updated.{Environment.NewLine} {ex}");
                }
                tMeta.Commit();
            }
        }

        /// <summary>
        /// Synchronizes the local model. Pushes the updated metadata to the central model.
        /// </summary>
        public void SyncCloudModel()
        {
            TransactWithCentralOptions tOpt = new TransactWithCentralOptions();
            RelinquishOptions rOpt = new RelinquishOptions(true);
            rOpt.StandardWorksets = true;
            rOpt.ViewWorksets = true;
            rOpt.FamilyWorksets = true;
            rOpt.UserWorksets = true;
            rOpt.CheckedOutElements = true;
            SynchronizeWithCentralOptions syncOpt = new SynchronizeWithCentralOptions();
            syncOpt.SetRelinquishOptions(rOpt);
            syncOpt.Compact = false;
            syncOpt.SaveLocalBefore = true;
            syncOpt.SaveLocalAfter = true;
            Doc.SynchronizeWithCentral(tOpt, syncOpt);
        }

        /// <summary>
        /// Saves a clean version of the model in a given location.
        /// </summary>
        /// <param name="targetDir"></param>
        public void SaveIssueModel(string targetDir, string fileName)
        {
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
            string filePath = $"{targetDir}\\{fileName}.rvt";
            
            WorksharingSaveAsOptions wsOpt = new WorksharingSaveAsOptions();

            SaveAsOptions sOpt = new SaveAsOptions();
            sOpt.SetWorksharingOptions(wsOpt);
            sOpt.OverwriteExistingFile = true;
            sOpt.MaximumBackups = 1;
            sOpt.Compact = false;
            Doc.SaveAs(filePath, sOpt);

            TransactWithCentralOptions tOpt = new TransactWithCentralOptions();
            RelinquishOptions rOpt = new RelinquishOptions(true);
            rOpt.StandardWorksets = true;
            rOpt.ViewWorksets = true;
            rOpt.FamilyWorksets = true;
            rOpt.UserWorksets = true;
            rOpt.CheckedOutElements = true;

            SynchronizeWithCentralOptions syncOpt = new SynchronizeWithCentralOptions();
            syncOpt.SetRelinquishOptions(rOpt);
            syncOpt.Compact = false;
            syncOpt.SaveLocalBefore = false;
            syncOpt.SaveLocalAfter = false;

            Doc.SynchronizeWithCentral(tOpt, syncOpt);
        }
        
        public void SelectFolder(string folderPath)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.SelectedPath = folderPath;
            dialog.ShowDialog();
        }
        
        private bool CheckForCoordSheets(Document doc)
        {
            var allSheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().Where(s => !s.SheetNumber.Contains("Project Information")).ToList();
            bool result = false;
            foreach (ViewSheet sheet in allSheets)
            {
                if (sheet.LookupParameter("Sheet Phase").AsString().Contains("COORD"))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        private bool CheckForCoordViews(Document doc)
        {
            List<View> allViews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().
                Where(v => v.Name != "IFC Export").Where(v => v.Name != "Navisworks").
                Cast<View>().ToList();
            List<View> viewsToProcess = new List<View>();
            bool result = false;


            foreach (View view in allViews)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue) continue;
                else
                {
                    if (view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        result = true;
                        break;
                    }
                }

                List<View> allSchedules = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Schedules).WhereElementIsNotElementType().Cast<View>().ToList();
                foreach (View sch in allSchedules)
                {
                    if (!sch.LookupParameter("View Folder 1 (View type)").HasValue)
                    {
                        continue;
                    }
                    else
                    {
                        if (sch.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        public static List<ElementId> DeleteNonCoordViews(Document doc)
        {
            List<ElementId> viewIdsToDelete = new List<ElementId>();

            List<ViewPlan> allViewPlans = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().Cast<ViewPlan>().ToList();
            foreach (ViewPlan view in allViewPlans)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue)
                {
                    viewIdsToDelete.Add(view.Id);
                }
                else
                {
                    if (!view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        viewIdsToDelete.Add(view.Id);
                    }
                }
            }
            List<ViewSection> allViewSec = new FilteredElementCollector(doc).OfClass(typeof(ViewSection)).WhereElementIsNotElementType().Cast<ViewSection>().ToList();
            foreach (ViewSection view in allViewSec)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue)
                {
                    viewIdsToDelete.Add(view.Id);
                }
                else
                {
                    if (!view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        viewIdsToDelete.Add(view.Id);
                    }
                }
            }
            List<View3D> all3D = new FilteredElementCollector(doc).OfClass(typeof(View3D)).WhereElementIsNotElementType().Cast<View3D>().ToList();
            foreach (View3D view in all3D)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue)
                {
                    viewIdsToDelete.Add(view.Id);
                }
                else
                {
                    if (!view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        viewIdsToDelete.Add(view.Id);
                    }
                }
            }
            List<ViewDrafting> allViewDraft = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).WhereElementIsNotElementType().Cast<ViewDrafting>().ToList();
            foreach (ViewDrafting view in allViewDraft)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue)
                {
                    viewIdsToDelete.Add(view.Id);
                }
                else
                {
                    if (!view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        viewIdsToDelete.Add(view.Id);
                    }
                }
            }
            List<TableView> allTableView = new FilteredElementCollector(doc).OfClass(typeof(TableView)).WhereElementIsNotElementType().Cast<TableView>().ToList();
            foreach (TableView view in allTableView)
            {
                if (!view.LookupParameter("View Folder 1 (View type)").HasValue)
                {
                    viewIdsToDelete.Add(view.Id);
                }
                else
                {
                    if (!view.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        viewIdsToDelete.Add(view.Id);
                    }
                }
            }
            List<View> allLegends = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Where(v => v.ViewType == ViewType.Legend).ToList();
            foreach (var leg in allLegends)
            {
                if (!leg.LookupParameter("View Folder 1 (View type)").HasValue)
                {
                    viewIdsToDelete.Add(leg.Id);
                }
                else
                {
                    if (!leg.LookupParameter("View Folder 1 (View type)").AsString().Contains("COORD"))
                    {
                        viewIdsToDelete.Add(leg.Id);
                    }
                }
            }
            return viewIdsToDelete;
        }
 
        //TODO: Allow the user to modify at least some of the settings
        /// <summary>
        /// Exports IFC file with predefined settings
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="fileName"></param>
        public void ExportIFC(string dirPath, string fileName)
        {
            ProjectInfo prjInfo = new FilteredElementCollector(Doc)
                .OfClass(typeof(ProjectInfo))
                .Cast<ProjectInfo>()
                .FirstOrDefault();

            View IFCview = new FilteredElementCollector(Doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .Where(v => v.Name == "IFC Export")
                .FirstOrDefault();

            IFCExportOptions ifcOpt = new IFCExportOptions();
            ifcOpt.FileVersion = IFCVersion.IFC2x3;
            ifcOpt.WallAndColumnSplitting = false;
            ifcOpt.ExportBaseQuantities = false;
            ifcOpt.FilterViewId = IFCview.Id;

            ifcOpt.AddOption("ExportInternalRevitPropertySets", "false");
            ifcOpt.AddOption("ExportIFCCommonPropertySets", "true");
            ifcOpt.AddOption("ExportAnnotations ", "true");
            ifcOpt.AddOption("SpaceBoundaries ", "0");
            ifcOpt.AddOption("ExportRoomsInView", "true");
            ifcOpt.AddOption("Use2DRoomBoundaryForVolume ", "true");
            ifcOpt.AddOption("UseFamilyAndTypeNameForReference ", "true");
            ifcOpt.AddOption("Export2DElements", "false");
            ifcOpt.AddOption("ExportPartsAsBuildingElements", "false");
            ifcOpt.AddOption("ExportBoundingBox", "false");
            ifcOpt.AddOption("ExportSolidModelRep", "true");
            ifcOpt.AddOption("ExportSchedulesAsPsets", "false");
            ifcOpt.AddOption("ExportSpecificSchedules", "false");
            ifcOpt.AddOption("ExportLinkedFiles", "false");
            ifcOpt.AddOption("IncludeSiteElevation", "true");
            ifcOpt.AddOption("StoreIFCGUID", "true");
            ifcOpt.AddOption("VisibleElementsOfCurrentView ", "true");
            ifcOpt.AddOption("UseActiveViewGeometry", "true");
            ifcOpt.AddOption("TessellationLevelOfDetail", "0,5");
            ifcOpt.AddOption("ExportUserDefinedPsets", "false");
            ifcOpt.AddOption("SitePlacement", "0");
            Doc.Export(dirPath, fileName, ifcOpt);
        }
        public static void ExportNWC(Document doc)
        {
            var prjInfo = new FilteredElementCollector(doc).OfClass(typeof(ProjectInfo)).Cast<ProjectInfo>().FirstOrDefault();
            string prjDir = prjInfo.LookupParameter("Project Directory").AsString();
            DateTime today = DateTime.Today;
            string date = today.ToString("yyMMdd");
            string dirPath = $"{prjDir}\\01 WIP - Internal Work\\{date}";
            string fileName = doc.Title.Split('_')[0];

            try
            {
                var NWCview = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().FirstOrDefault(v => v.Name == "Navisworks");
                NavisworksExportOptions nwcOpt = new NavisworksExportOptions();
                nwcOpt.ExportScope = NavisworksExportScope.View;
                nwcOpt.ViewId = NWCview.Id;
                nwcOpt.ExportLinks = false;
                nwcOpt.Coordinates = 0;
                nwcOpt.ExportElementIds = true;
                nwcOpt.ConvertElementProperties = true;
                nwcOpt.ExportRoomAsAttribute = true;
                nwcOpt.ExportRoomGeometry = false;

                doc.Export(dirPath, fileName, nwcOpt);
            }
            catch { }
        }
        public static void PurgeModel(Document doc)
        {
            PerformanceAdviser perfAd = PerformanceAdviser.GetPerformanceAdviser();
            var allRulesList = perfAd.GetAllRuleIds();
            var rulesToExecute = new List<PerformanceAdviserRuleId>();
            foreach (PerformanceAdviserRuleId r in allRulesList)
            {
                if (perfAd.GetRuleName(r).Equals("Project contains unused families and types"))
                {
                    rulesToExecute.Add(r);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                IList<FailureMessage> failMessages = perfAd.ExecuteRules(doc, rulesToExecute);
                if (failMessages.Count() == 0) return;

                ICollection<ElementId> failingElementIds = failMessages[0].GetFailingElements();
                foreach (ElementId id in failingElementIds)
                {
                    try
                    {
                        doc.Delete(id);
                    }
                    catch { }
                }
            }
        }
        public static string GetProjectLeadInitials(Document doc)
        {
            string plInitials = string.Empty;
            try
            {
                ProjectInfo pi = new FilteredElementCollector(doc)
                    .OfClass(typeof(ProjectInfo))
                    .Cast<ProjectInfo>()
                    .FirstOrDefault();
                if (pi.LookupParameter("ACG_Project Lead")?.AsValueString() != string.Empty)
                {
                    string plName = pi.LookupParameter("ACG_Project Lead").AsString();
                    if (plName.Count() <= 3) plInitials = plName;
                    else plInitials = string.Concat(plName.Where(char.IsUpper));
                }
            }
            catch (Exception ex) { }

            return plInitials;
        }
        public static string GetUserInitials(UIApplication uiApp)
        {
            string userInitials = string.Empty;
            try
            {
                char first = uiApp.Application.Username.Split('.')[0][0];
                char second = uiApp.Application.Username.Split('.')[1][0];
                userInitials = string.Concat(char.ToUpper(first), char.ToUpper(second));
            }
            catch { }
            return userInitials;
        }
        public static void SuspendWarnings(UIControlledApplication application)
        {
            // call our method that will load up our toolbar
            application.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);
        }
        public static void UnSuspendWarnings(UIControlledApplication application)
        {
            // call our method that will load up our toolbar
            application.ControlledApplication.FailuresProcessing -= new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);
        }
    }
}
