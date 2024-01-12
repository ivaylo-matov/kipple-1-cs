using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.UI.Selection;

namespace Purge_Rooms_UI.PlaceFaceBasedFam
{
    public partial class PlaceFaceBasedFamView : Window
    {
        public UIDocument uiDoc { get; }
        public Document doc { get; }

        private ExternalEvent externalEvent;

        public PlaceFaceBasedFamView(UIDocument UiDoc)
        {
            uiDoc = UiDoc;
            doc = UiDoc.Document;

            InitializeComponent();

            externalEvent = ExternalEvent.Create(new PlaceFamilyExternalEventHandler());
        }

        private void SelectAndPlaceFamily(object sender, RoutedEventArgs e)
        {
            try { externalEvent.Raise(); }
            catch (Exception ex) { TaskDialog.Show("Error", $"Error: {ex.Message}"); }
        }

        private class PlaceFamilyExternalEventHandler : IExternalEventHandler
        {
            public void Execute(UIApplication app)
            {
                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc.Document;

                try
                {
                    Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Face, "Select a face");
                    ElementId wallId = pickedRef.ElementId;
                    Element wall = doc.GetElement(wallId);

                    if (wall != null && wall is Wall)
                    {
                        Options options = new Options();
                        options.ComputeReferences = true;

                        Face wallFace = wall.GetGeometryObjectFromReference(pickedRef) as Face;
                        if (wallFace != null) PlaceFamilyInstanceOnFace(doc, wallFace, pickedRef);
                    }
                }
                catch (Exception ex) { TaskDialog.Show("Error", $"Error: {ex.Message}"); }
            }

            public string GetName()
            {
                return "PlaceFamilyExternalEventHandler";
            }

            /// <summary>
            /// Places instance of a given symbol on a face
            /// </summary>
            /// <param name="doc"></param>
            /// <param name="face"></param>
            private void PlaceFamilyInstanceOnFace(Document doc, Face face, Reference faceRef)
            {
                // Get the center of the wallFace
                BoundingBoxUV boundingBox = face.GetBoundingBox();
                UV center = (boundingBox.Max + boundingBox.Min) * 0.5;
                XYZ location = face.Evaluate(center);

                // Get the direction of the wallFace
                XYZ refDir = face.ComputeNormal(center).CrossProduct(XYZ.BasisZ);

                // Place the family instance
                using (Transaction transaction = new Transaction(doc, "Place Family Instance"))
                {
                    transaction.Start();
                    if (doc.GetElement(faceRef) != null)
                    {
                        doc.Create.NewFamilyInstance(faceRef, location, refDir, GetSymbol(doc));
                    }                    
                    transaction.Commit();
                }
            }

            /// <summary>
            /// Get the first face based symbol in the model
            /// </summary>
            /// <param name="doc"></param>
            /// <returns></returns>
            private FamilySymbol GetSymbol(Document doc)
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