using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using Autodesk.Revit.DB.Events;

namespace Purge_Rooms_UI
{
    public partial class IssueModelView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public UIApplication uiapp { get; }
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
            string sync = IssueModelCommand.SyncCloudModel(doc);

            using (Transaction cleanModel = new Transaction(doc, "Clean Model"))
            {
                //// failure handling options
                FailureHandlingOptions failtOpt = cleanModel.GetFailureHandlingOptions();
                FailurePrerocessor prerocessor = new FailurePrerocessor();
                failtOpt.SetFailuresPreprocessor(prerocessor);
                cleanModel.SetFailureHandlingOptions(failtOpt);

                // start transaction
                cleanModel.Start();            

                if (chkRVTlinks.IsChecked == true)
                {
                    string removeRVT = IssueModelCommand.RemoveRVTLins(doc);
                }
                if (chkCADlinks.IsChecked == true)
                {
                    string removeCAD = IssueModelCommand.RemoveCADLins(doc);
                }
                if (chkPDFlinks.IsChecked == true)
                {
                    string removeImage = IssueModelCommand.RemovePDFLins(doc);
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
                    string removeGroups = IssueModelCommand.UngroupAllGroups(doc);
                }
                if (chkLibraryPhase.IsChecked == true)
                {
                    string removeLibrary = IssueModelCommand.DeleteLibraryElements(doc);
                }
                if (chkIFC.IsChecked == true)
                {
                    string exportIFC = IssueModelCommand.ExportIFC(doc);
                }

                IssueModelCommand.PurgeModel(doc);

                cleanModel.Commit();

                Close();
            }
            if (chkNWC.IsChecked == true)
            {
                string exportNWC = IssueModelCommand.ExportNWC(doc);
            }

            string save = IssueModelCommand.SaveIssueModel(doc);
        }



    }
}
