using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;

namespace Purge_Rooms_UI
{
    /// <summary>
    /// Interaction logic for Isolatetest.xaml
    /// </summary>
    public partial class Isolatetest : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }
        public Isolatetest(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();
            Title = "Isolate Warnings By Type";

            IList<string> warnings = IsolateWarningsByTypeCommand.GetInput(doc);
            warningTypes.ItemsSource = warnings;
        }

        private void IsolatetestClick(object sender, RoutedEventArgs e)
        {
            using (Transaction t = new Transaction(doc, "Isolate warnings by type"))
            {
                t.Start();
                string selection = warningTypes.SelectedItem as string;
                if (!string.IsNullOrEmpty(selection))
                {
                    try
                    {
                        IList<ElementId> idsToIsolate = new List<ElementId>();
                        string selectedWarning = selection.Split(':')[1];
                        selectedWarning = selectedWarning.Substring(1);
                        IList<FailureMessage> allWarnings = doc.GetWarnings();
                        foreach (FailureMessage fail in allWarnings)
                        {
                            if (fail.GetDescriptionText().Contains(selectedWarning))
                            {
                                var elementIds = fail.GetFailingElements();
                                foreach (ElementId id in elementIds)
                                {
                                    idsToIsolate.Add(id);
                                }
                            }
                        }

                        if (doc.ActiveView.IsTemporaryHideIsolateActive())
                        {
                            TemporaryViewMode temp = TemporaryViewMode.TemporaryHideIsolate;
                            doc.ActiveView.DisableTemporaryViewMode(temp);
                        }

                        doc.ActiveView.IsolateElementsTemporary(idsToIsolate);
                    }
                    catch { }
                }
                t.Commit();
                Close();
            }  
        }
    }
}
