using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.UI.Selection;
using static Purge_Rooms_UI.PlaceFaceBasedFam.PlaceFaceBasedFamCommand;

namespace Purge_Rooms_UI.PlaceFaceBasedFam
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PlaceFaceBasedFamView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }

        private ExternalEvent externalEvent;

        public PlaceFaceBasedFamView(UIDocument UiDoc)
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();

            externalEvent = ExternalEvent.Create(new PlaceFamilyExternalEventHandler());
        }

        private void SelectAndPlaceFamily(object sender, RoutedEventArgs e)
        {
            try
            {
                // Trigger the external event
                externalEvent.Raise();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class PlaceFamilyExternalEventHandler : IExternalEventHandler
        {
            public void Execute(UIApplication app)
            {
                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc.Document;

                try
                {
                    Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Select a wall");
                    Element wall = doc.GetElement(pickedRef.ElementId);

                    if (wall != null && wall is Wall)
                    {
                        Options options = new Options();
                        options.ComputeReferences = true;

                        GeometryElement geometryElement = wall.get_Geometry(options);

                        foreach (GeometryObject geometryObject in geometryElement)
                        {
                            if (geometryObject is Solid solid)
                            {
                                foreach (Face wallFace in solid.Faces)
                                {
                                    // You may want to add additional checks to ensure you are selecting the desired face
                                    // For example, check face normal or other properties
                                    PlaceFamilyInstanceOnWall(doc, wallFace);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"Error: {ex.Message}");
                }
            }

            public string GetName()
            {
                return "PlaceFamilyExternalEventHandler";
            }

            private void PlaceFamilyInstanceOnWall(Document doc, Face wallFace)
            {
                // Get the family type
                FamilySymbol symbol = GetFamilyType(doc);

                // Get the center of the wallFace
                BoundingBoxUV boundingBox = wallFace.GetBoundingBox();
                UV center = (boundingBox.Max + boundingBox.Min) * 0.5;
                XYZ location = wallFace.Evaluate(center);

                // Get the direction of the wallFace
                XYZ normal = wallFace.ComputeNormal(center);
                XYZ refDir = normal.CrossProduct(XYZ.BasisZ);

                // Place the family instance
                using (Transaction transaction = new Transaction(doc, "Place Family Instance"))
                {
                    transaction.Start();
                    FamilyInstance instance = doc.Create.NewFamilyInstance(wallFace, location, refDir, symbol);
                    transaction.Commit();
                }

                TaskDialog.Show("Selected Wall Face", $"Placed family instance on Wall {doc.GetElement(wallFace.Reference).Name}");
            }

            private FamilySymbol GetFamilyType(Document doc)
            {
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(f => f.FamilyName.Contains("FaceBased"))
                    .FirstOrDefault();
            }
        }
    }
}