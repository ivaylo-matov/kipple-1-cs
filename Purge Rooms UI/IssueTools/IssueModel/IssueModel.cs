// Ignore Spelling: Coord Preprocess

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
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
        private static List<View> LogAllViews = new List<View>();
        private static List<ElementId> LogCoordViewIds = new List<ElementId>();
        private static List<Phase> LogLibPhase = new List<Phase>();
        private static List<Group> LogModelGroups = new List<Group>();

        public static string TargetFolderNotFoundMessage = "Not found! Please navigate to target folder.";

        #region Enable/Disable CheckBoxes
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
            LogAllViews = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.Name != "IFC Export")
                .Where(v => v.Name != "NWC Export")
                .Where(v => v.Id != GetSplachScreen().Id)
                .ToList();
            return LogAllViews.Count > 0;
        }
        public bool EnableCoordViews()
        {
            List <ElementId> coordViewIds = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(v => v.Name.ToLower().Contains("_coord"))
                .Select(v => v.Id)
                .ToList();

            List<ElementId> coordSheetIds = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(s => s.Name.ToLower().Contains("_coord"))
                .Where(s => s != GetSplachScreen())
                .Select(s => s.Id)
                .ToList();

            List<ElementId> viewsOnSheetIds = new List<ElementId>();
            foreach (ElementId id in coordSheetIds)
            {
                ViewSheet sheet = Doc.GetElement(id) as ViewSheet;
                viewsOnSheetIds.AddRange(sheet.GetAllPlacedViews());
            }

            LogCoordViewIds.AddRange(coordViewIds);
            LogCoordViewIds.AddRange(coordSheetIds);
            LogCoordViewIds.AddRange(viewsOnSheetIds);

            return LogCoordViewIds.Count > 0;
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
        #endregion

        #region Update / Sync / Save
        /// <summary>
        /// Looks at the Splash Page, which should be the active view and returns the data from the latest revision.
        /// It also looks for the project directory in Project Information.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CollectCurrentMetaData()
        {
            ViewSheet splashScr = GetSplachScreen();

            //TODO: this breaks if the active view is not a SHEET. Handle this.
            string approvedByValue = splashScr.get_Parameter(BuiltInParameter.SHEET_APPROVED_BY).AsValueString();
            if (approvedByValue == "Approver") approvedByValue = "";

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
                { "ApprovedBy", approvedByValue },
                { "RevDescription", lastRev.Description },
                { "TargetDir", targetDir},
                { "TargetFileName", GetCentralModelName()}
                };
            }
            else
            {
                return new Dictionary<string, string> {
                { "IssuedTo", "" },
                { "IssuedBy", GetUserInitials() },
                { "ApprovedBy", "" },
                { "RevDescription", "" },
                { "TargetDir", targetDir},
                { "TargetFileName", GetCentralModelName()}
                };
            }
        }
        private string GetUserInitials()
        {
            string userInitials = string.Empty;
            try
            {
                char first = UIApp.Application.Username.Split('.')[0][0];
                char second = UIApp.Application.Username.Split('.')[1][0];
                userInitials = string.Concat(char.ToUpper(first), char.ToUpper(second));
            }
            catch { }
            return userInitials;
        }
        public string GetCentralModelName()
        {
            return Doc.Title.Replace($"_{UIApp.Application.Username}", "");
        }
        public void UpdateMetaData(string revDescription, string issuedBy, string issuedTo, string approvedBy)
        {
            using (Transaction tMeta = new Transaction(Doc, "Update metadata"))
            {
                tMeta.Start();
                try
                {
                    ViewSheet splashScr = GetSplachScreen();

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

        // Tries to get the Splash Screen. Users should be asked to have it as active view
        // ...but if they click on the Project Browser or Properties Palette, the core will return
        // ...exception. In that case, this method will return the first open view of the document
        // ...in hope that this indeed is the Splash Screen
        private ViewSheet GetSplachScreen()
        {
            ViewSheet splashScr = null;
            var uiViews = UIApp.ActiveUIDocument.GetOpenUIViews();

            if (Doc.ActiveView != null && Doc.ActiveView is ViewSheet) splashScr = Doc.ActiveView as ViewSheet;
            else
            {
                splashScr = uiViews.Select(v => Doc.GetElement(v.ViewId) as View)
                .Where(v => v != null && v is ViewSheet)
                .Cast<ViewSheet>()
                .FirstOrDefault();
            }
            return splashScr;
        }
        public string ReportActiveView()
        {
            if (Doc.ActiveView is ViewSheet activeSheet)
            {
                return $"{activeSheet.SheetNumber} - {activeSheet.Name}";
            }
            return Doc.ActiveView?.Name;
        }
        #endregion

        #region Clean

        public void RemoveAllViews()
        {
            using (Transaction tViews = new Transaction(Doc, "Remove Sheets & Views"))
            {
                tViews.Start();

                // register to hide warnings
                FailureHandlingOptions options = tViews.GetFailureHandlingOptions();
                HideWarnings hideWarnings = new HideWarnings();
                options.SetFailuresPreprocessor(hideWarnings);
                tViews.SetFailureHandlingOptions(options);

                foreach (var i in LogAllViews)
                {
                    try { Doc.Delete(i.Id); }
                    catch { }
                }
                tViews.Commit();
            }
        }
        public void RemoveNonCoordViews()
        {
            List<ElementId> nonProtectedViews = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .Where(v => v.Name != "IFC Export" && v.Name != "NWC Export" && v != GetSplachScreen())
                .Cast<View>()
                .Select(v => v.Id)
                .ToList();

            using (Transaction tViews = new Transaction(Doc, "Remove Non Coordination Sheets & Views"))
            {
                tViews.Start();

                // register to hide warnings
                FailureHandlingOptions options = tViews.GetFailureHandlingOptions();
                HideWarnings hideWarnings = new HideWarnings();
                options.SetFailuresPreprocessor(hideWarnings);
                tViews.SetFailureHandlingOptions(options);

                foreach (ElementId id in nonProtectedViews)
                {
                    if (!LogCoordViewIds.Contains(id))
                    {
                        try { Doc.Delete(id); }
                        catch { }
                    }
                }
                tViews.Commit();
            }
        }
        public void RemoveLibPhaseElements()
        {
            using (Transaction tLib = new Transaction(Doc, "Remove Elements from Library phase"))
            {
                tLib.Start();

                // register to hide warnings
                FailureHandlingOptions options = tLib.GetFailureHandlingOptions();
                HideWarnings hideWarnings = new HideWarnings();
                options.SetFailuresPreprocessor(hideWarnings);
                tLib.SetFailureHandlingOptions(options);

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

                // register to hide warnings
                FailureHandlingOptions options = tGroup.GetFailureHandlingOptions();
                HideWarnings hideWarnings = new HideWarnings();
                options.SetFailuresPreprocessor(hideWarnings);
                tGroup.SetFailureHandlingOptions(options);

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
        public void PurgeModel_Old()
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

            using (Transaction tPurge = new Transaction(Doc, "Purge"))
            {
                tPurge.Start();

                for (int i = 0; i < 3; i++)
                {
                    IList<FailureMessage> failMessages = perfAd.ExecuteRules(Doc, rulesToExecute);
                    if (failMessages.Count() == 0) return;

                    ICollection<ElementId> failingElementIds = failMessages[0].GetFailingElements();
                    foreach (ElementId id in failingElementIds)
                    {
                        try
                        {
                            Doc.Delete(id);
                        }
                        catch { }
                    }
                }
                Doc.Regenerate();
                for (int i = 0; i < 3; i++)
                {
                    IList<FailureMessage> failMessages = perfAd.ExecuteRules(Doc, rulesToExecute);
                    if (failMessages.Count() == 0) return;

                    ICollection<ElementId> failingElementIds = failMessages[0].GetFailingElements();
                    foreach (ElementId id in failingElementIds)
                    {
                        try
                        {
                            Doc.Delete(id);
                        }
                        catch { }
                    }
                }

                tPurge.Commit();
            }
        }
        public void PurgeModel()
        {
            using (Transaction tPurge = new Transaction(Doc, "Purge"))
            {
                tPurge.Start();

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
                for (int i = 0; i < 10; i++)
                {
                    IList<FailureMessage> failMessages = perfAd.ExecuteRules(Doc, rulesToExecute);
                    if (failMessages.Count() == 0) return;

                    ICollection<ElementId> failingElementIds = failMessages[0].GetFailingElements();
                    foreach (ElementId id in failingElementIds)
                    {
                        try { Doc.Delete(id); }
                        catch { }
                    }
                }  
                tPurge.Commit();
            }
        }

        #endregion


        #region Export
        //TODO: Allow the user to modify at least some of the settings
        public void ExportIFC(string dirPath, string fileName)
        {
            ProjectInfo prjInfo = new FilteredElementCollector(Doc)
                .OfClass(typeof(ProjectInfo))
                .Cast<ProjectInfo>()
                .FirstOrDefault();

            // get the IFC Export view
            View IFCview = new FilteredElementCollector(Doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .Where(v => v.Name == "IFC Export")
                .FirstOrDefault();

            IFCExportOptions ifcOpt = new IFCExportOptions();
            ifcOpt.FileVersion = IFCVersion.IFC2x3;
            ifcOpt.WallAndColumnSplitting = false;
            ifcOpt.ExportBaseQuantities = false;
            // if there is no IFC Export view, all elements in the model will be exported
            if (IFCview != null) ifcOpt.FilterViewId = IFCview.Id;

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

            using (Transaction tIFC = new Transaction(Doc, "Export IFC"))
            {
                tIFC.Start();
                Doc.Export(dirPath, fileName, ifcOpt);
                tIFC.Commit();
            }                
        }

        //TODO: Note that NWC Export Utility must be installed
        public void ExportNWC(string dirPath, string fileName)
        {
            ProjectInfo prjInfo = new FilteredElementCollector(Doc)
                .OfClass(typeof(ProjectInfo))
                .Cast<ProjectInfo>()
                .FirstOrDefault();

            View NWCview = new FilteredElementCollector(Doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                .FirstOrDefault(v => v.Name == "NWC Export");

            NavisworksExportOptions nwcOpt = new NavisworksExportOptions();
            nwcOpt.ExportScope = NavisworksExportScope.View;
            if (NWCview != null) nwcOpt.ViewId = NWCview.Id;
            nwcOpt.ExportLinks = false;
            nwcOpt.Coordinates = 0;
            nwcOpt.ExportElementIds = true;
            nwcOpt.ConvertElementProperties = true;
            nwcOpt.ExportRoomAsAttribute = true;
            nwcOpt.ExportRoomGeometry = false;

            Doc.Export(dirPath, fileName, nwcOpt);
        }
        #endregion


        /// <summary>
        /// Hides any warnings Revit might throw back but passes Errors to the user
        /// </summary>
        public class HideWarnings : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failureAccelerator)
            {
                IList<FailureMessageAccessor> failureMessages = failureAccelerator.GetFailureMessages();
                foreach (FailureMessageAccessor failureMessage in failureMessages)
                {
                    if (failureMessage.GetDescriptionText().Contains("Last member of group instance was excluded (deleted)"))
                    {
                        failureMessage.SetCurrentResolutionType(FailureResolutionType.Default);
                        failureAccelerator.ResolveFailure(failureMessage);
                    }
                    else if (failureMessage.GetSeverity() == FailureSeverity.Warning)
                    {
                        failureAccelerator.DeleteWarning(failureMessage);
                    }
                    else if (failureMessage.GetSeverity() == FailureSeverity.Error)
                    {
                        if (failureMessage.GetDescriptionText().Contains("joined"))
                        {
                            failureMessage.SetCurrentResolutionType(FailureResolutionType.DetachElements);
                            failureAccelerator.ResolveFailure(failureMessage);
                        }
                    }
                }
                return FailureProcessingResult.Continue;
            }
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
