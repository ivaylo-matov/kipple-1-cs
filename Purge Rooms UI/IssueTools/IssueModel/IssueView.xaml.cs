namespace Purge_Rooms_UI
{
    public partial class IssueView
    {
 
        public IssueView()
        {
            InitializeComponent();  
        }

//        private void IssueModel(object sender, RoutedEventArgs e)
//        {
//            // SUBSCRIBE TO THE EVENT HANDLER HERE ???
//            _uiContrApp.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);

//            using (Transaction updateMetadata = new Transaction(doc, "Update splash"))
//            {
//                updateMetadata.Start();
//                // collect values
//                string issuedTo = issuedToBox.Text;
//                string issuedBy = issuedByBox.Text;
//                string approvedBy = approvedByBox.Text;
//                string revDescrib = revDescribBox.Text;
//                string statusCode = statusBox.Text.Split('-')[0];
//                string statusDesc = statusBox.Text.Split('-')[1];

//                ViewSheet splash = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet))
//                    .Cast<ViewSheet>()
//                    .FirstOrDefault(s => s.SheetNumber == "Project Information");
                
//                Revision rev = Revision.Create(doc);

//                rev.Description = revDescribBox.Text;
//                rev.IssuedTo = issuedByBox.Text;
//                rev.RevisionDate = DateTime.Today.ToString("dd.MM.yy");

//                ICollection<ElementId> currentRevs = splash.GetAdditionalRevisionIds();
//                currentRevs.Add(rev.Id);

//                splash.SetAdditionalRevisionIds(currentRevs);
//                splash.LookupParameter("Drawn By").Set(issuedBy);
//                splash.LookupParameter("Approved By").Set(approvedBy);
//                splash.LookupParameter("Suitability No.").Set(statusCode);
//                splash.LookupParameter("Suitability Desc.").Set(statusDesc);
//                updateMetadata.Commit();
//            }
//;
//            IssueCommand.SyncCloudModel(doc);

//            using (Transaction cleanModel = new Transaction(doc, "Clean Model"))
//            {
//                //// failure handling options
//                FailureHandlingOptions failtOpt = cleanModel.GetFailureHandlingOptions();
//                FailurePrerocessor prerocessor = new FailurePrerocessor();
//                failtOpt.SetFailuresPreprocessor(prerocessor);
//                cleanModel.SetFailureHandlingOptions(failtOpt);

//                // start transaction
//                cleanModel.Start();

//                //IssueModelCommand.PurgeModel(doc);

//                if (chkRVTlinks.IsChecked == true)
//                {
//                    IssueCommand.RemoveRVTLinks(doc);
//                }
//                if (chkCADlinks.IsChecked == true)
//                {
//                    IssueCommand.RemoveCADLinks(doc);
//                }
//                if (chkPDFlinks.IsChecked == true)
//                {
//                    IssueCommand.RemovePDFLinks(doc);
//                }
//                if (chkCoordViews.IsChecked == true)
//                {
//                    List<ElementId> viewIdsToDelete = IssueCommand.DeleteNonCoordViews(doc);
//                    foreach (ElementId viewId in viewIdsToDelete)
//                    {
//                        try
//                        {
//                            doc.Delete(viewId);
//                        }
//                        catch { }
//                    }
//                    List<ElementId> sheetIdsToDelete = IssueCommand.DeleteNonCoordSheets(doc);
//                    foreach (ElementId sheetId in sheetIdsToDelete)
//                    {
//                        try
//                        {
//                            doc.Delete(sheetId);
//                        }
//                        catch { }
//                    }
//                }
//                else if (chkViews.IsChecked == true)
//                {
//                    List<ElementId> viewIdsToDelete = IssueCommand.DeleteAllViews(doc);
//                    foreach (ElementId viewId in viewIdsToDelete)
//                    {
//                        try
//                        {
//                            doc.Delete(viewId);
//                        }
//                        catch { }
//                    }
//                    List<ElementId> sheetIdsToDelete = IssueCommand.DeleteAllSheets(doc);
//                    foreach (ElementId sheetId in sheetIdsToDelete)
//                    {
//                        try
//                        {
//                            doc.Delete(sheetId);
//                        }
//                        catch { }
//                    }
//                }
//                if (chkGroups.IsChecked == true)
//                {
//                    IssueCommand.UngroupAllGroups(doc);
//                }
//                if (chkLibraryPhase.IsChecked == true)
//                {
//                    IssueCommand.DeleteLibraryElements(doc);
//                }
//                if (chkIFC.IsChecked == true)
//                {
//                    IssueCommand.ExportIFC(doc);
//                }

//                IssueCommand.PurgeModel(doc);

//                cleanModel.Commit();

//                Close();
//            }
//            if (chkNWC.IsChecked == true)
//            {
//                IssueCommand.ExportNWC(doc);
//            }

//            IssueCommand.SaveIssueModel(doc);

//            _uiContrApp.ControlledApplication.FailuresProcessing -= new EventHandler<FailuresProcessingEventArgs>(FailurePreprocessor_Event.ProcessFailuresEvents);
//        }
    }
}
