﻿using Autodesk.Revit.DB;
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


        public void Run()
        {

        }

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
                .Where(v => v.Id != Doc.ActiveView.Id)
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
                .Where(s => s != Doc.ActiveView)
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

        /// <summary>
        /// Removes all views and sheets from LogAllViews
        /// </summary>
        public void RemoveAllViews()
        {
            using (Transaction tViews = new Transaction(Doc, "Remove Sheets & Views"))
            {
                tViews.Start();
                foreach (var i in LogAllViews)
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

        /// <summary>
        /// Un-groups all detail and model groups
        /// </summary>
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

        /// <summary>
        /// Removes views and sheets which name does not contain _COORD
        /// </summary>
        public void RemoveNonCoordViews()
        {
            List<ElementId> nonProtectedViews = new FilteredElementCollector(Doc)
                .OfClass(typeof(View))                
                .WhereElementIsNotElementType()
                .Where(v => v.Name != "IFC Export" && v.Name != "NWC Export" && v != Doc.ActiveView)
                .Cast<View>()
                .Select(v => v.Id)
                .ToList();

            using (Transaction tViews = new Transaction(Doc, "Remove Non Coordination Sheets & Views"))
            {
                tViews.Start();
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

        /// <summary>
        /// Looks at the Splash Page, which should be the active view and returns the data from the latest revision.
        /// It also looks for the project directory in Project Information.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CollectCurrentMetaData()
        {
            ViewSheet splashScr = Doc.ActiveView as ViewSheet;
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


 
        //TODO: DOES NOT WORK!!!!
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
            Doc.Export(dirPath, fileName, ifcOpt);
        }

        //TODO: DOE NOT WORK!!!
        /// <summary>
        /// Exports NWC file with predefined settings
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="fileName"></param>
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

        /// <summary>
        /// Tries to assemble user initials from the Revit UserName
        /// </summary>
        /// <param name="uiApp"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the central model name if this is not Cloud Model, i.e. removes "_userName".
        /// </summary>
        /// <returns></returns>
        public string GetCentralModelName()
        {
            return Doc.Title.Replace($"_{UIApp.Application.Username}", "");
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
