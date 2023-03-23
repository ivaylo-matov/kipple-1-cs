using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;


namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for PurgeViewTemplatesView.xaml
    /// </summary>
    public partial class PurgeViewTemplatesView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public PurgeViewTemplatesView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;                

            InitializeComponent();
            Title = "Purge View Templates";
        }

        private void PurgeViewTemplates(object sender, RoutedEventArgs e)
        {
            int countDeleted = 0;
            using (Transaction t = new Transaction(doc, "Purge view templates"))
            {
                t.Start();

                if (chkTemplates.IsChecked == true)
                {
                    foreach (ElementId id in PurgeViewTemplatesCommand.GetUnusedViewTemplates(doc))
                    {
                        try
                        {
                            doc.Delete(id);
                            countDeleted++;
                        }
                        catch { }
                    }
                }
                t.Commit();
            }
            Close();

            if (countDeleted == 0) { TaskDialog.Show("Result", "No view templates were deleted."); }
            if (countDeleted == 1) { TaskDialog.Show("Result", "1 view template was deleted."); }
            if (countDeleted > 1) { TaskDialog.Show("Result", $"{countDeleted} view templates were deleted."); }
        }
    }
}
