using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

namespace Purge_Rooms_UI.PlaceFamily
{
    public class PlaceFamilyExternalEventHandler : IExternalEventHandler
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
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error: {ex.Message}");
            }
        }

        public string GetName()
        {
            return "PlaceFamilyExternalEventHandler";
        }

        private void PlaceFamilyInstanceOnFace(Document doc, Face face, Reference faceRef)
        {
            BoundingBoxUV boundingBox = face.GetBoundingBox();
            UV center = (boundingBox.Max + boundingBox.Min) * 0.5;
            XYZ location = face.Evaluate(center);

            XYZ refDir = face.ComputeNormal(center).CrossProduct(XYZ.BasisZ);

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

