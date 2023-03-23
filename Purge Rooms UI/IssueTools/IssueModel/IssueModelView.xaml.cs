using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.ApplicationServices;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Purge_Rooms_UI
{
    public partial class IssueModelView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public UIApplication uiapp { get; }
        public Application app { get; }
        public UIControlledApplication _uiContrApp { get; }
 
        public IssueModelView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Issue Model";
        }
        private void IssueModel(object sender, RoutedEventArgs e)
        {
            using (Transaction updateMetadata = new Transaction(doc, "Update splash"))
            {
                App.Instance.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);

                updateMetadata.Start();
                // colect values
                string issuedTo = issuedToBox.Text;
                string issuedBy = issuedByBox.Text;
                string approvedBy = approvedByBox.Text;
                string revDescrib = revDescribBox.Text;
                string statusCode = statusBox.Text.Split('-')[0];
                string statusDesc = statusBox.Text.Split('-')[1];

                ViewSheet splash = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .FirstOrDefault(s => s.SheetNumber == "Project Information");
                
                Revision rev = Revision.Create(doc);

                rev.Description = revDescribBox.Text;
                rev.IssuedTo = issuedByBox.Text;
                rev.RevisionDate = DateTime.Today.ToString("dd.MM.yy");

                ICollection<ElementId> currentRevs = splash.GetAdditionalRevisionIds();
                currentRevs.Add(rev.Id);

                splash.SetAdditionalRevisionIds(currentRevs);
                splash.LookupParameter("Drawn By").Set(issuedBy);
                splash.LookupParameter("Approved By").Set(approvedBy);
                splash.LookupParameter("Suitability No.").Set(statusCode);
                splash.LookupParameter("Suitability Desc.").Set(statusDesc);
                updateMetadata.Commit();
            }
;
            IssueModelCommand.SyncCloudModel(doc);

            using (Transaction cleanModel = new Transaction(doc, "Clean Model"))
            {
                //// failure handling options
                FailureHandlingOptions failtOpt = cleanModel.GetFailureHandlingOptions();
                FailurePrerocessor prerocessor = new FailurePrerocessor();
                failtOpt.SetFailuresPreprocessor(prerocessor);
                cleanModel.SetFailureHandlingOptions(failtOpt);

                // start transaction
                cleanModel.Start();

                //IssueModelCommand.PurgeModel(doc);

                if (chkRVTlinks.IsChecked == true)
                {
                    IssueModelCommand.RemoveRVTLinks(doc);
                }
                if (chkCADlinks.IsChecked == true)
                {
                    IssueModelCommand.RemoveCADLinks(doc);
                }
                if (chkPDFlinks.IsChecked == true)
                {
                    IssueModelCommand.RemovePDFLinks(doc);
                }
                if (chkCoordViews.IsChecked == true)
                {
                    List<ElementId> viewIdsToDelete = IssueModelCommand.DeleteNonCoordViews(doc);
                    foreach (ElementId viewId in viewIdsToDelete)
                    {
                        try
                        {
                            doc.Delete(viewId);
                        }
                        catch { }
                    }
                    List<ElementId> sheetIdsToDelete = IssueModelCommand.DeleteNonCoordSheets(doc);
                    foreach (ElementId sheetId in sheetIdsToDelete)
                    {
                        try
                        {
                            doc.Delete(sheetId);
                        }
                        catch { }
                    }
                }
                else if (chkViews.IsChecked == true)
                {
                    List<ElementId> viewIdsToDelete = IssueModelCommand.DeleteAllViews(doc);
                    foreach (ElementId viewId in viewIdsToDelete)
                    {
                        try
                        {
                            doc.Delete(viewId);
                        }
                        catch { }
                    }
                    List<ElementId> sheetIdsToDelete = IssueModelCommand.DeleteAllSheets(doc);
                    foreach (ElementId sheetId in sheetIdsToDelete)
                    {
                        try
                        {
                            doc.Delete(sheetId);
                        }
                        catch { }
                    }
                }
                if (chkGroups.IsChecked == true)
                {
                    IssueModelCommand.UngroupAllGroups(doc);
                }
                if (chkLibraryPhase.IsChecked == true)
                {
                    IssueModelCommand.DeleteLibraryElements(doc);
                }
                if (chkIFC.IsChecked == true)
                {
                    IssueModelCommand.ExportIFC(doc);
                }

                IssueModelCommand.PurgeModel(doc);

                cleanModel.Commit();

                Close();
            }
            if (chkNWC.IsChecked == true)
            {
                IssueModelCommand.ExportNWC(doc);
            }

            IssueModelCommand.SaveIssueModel(doc);

            App.Instance.ControlledApplication.FailuresProcessing -= new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);
        }
    }
}
