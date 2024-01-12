using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Purge_Rooms_UI.Bimorph;

namespace Purge_Rooms_UI.Bimorph
{
    public class BimorphModel
    {
        public UIApplication uiApp { get; }
        public Document doc { get; }
        public BimorphModel(UIApplication uiApp)
        {            
            doc = uiApp.ActiveUIDocument.Document;        
        }




        public void MainCode()
        {
            // get all valid rooms and floors
            List<SpatialElement> allRooms = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .WhereElementIsNotElementType()
                .Cast<SpatialElement>()
                .Where(x => x is Room)
                .Where(x => x.Area > 0)
            .ToList();

            List<Floor> allFloors = new FilteredElementCollector(doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .ToList();

            using (Transaction trans = new Transaction(doc, "Place elements"))
            {
                trans.Start();

                // get list of mapped floors
                foreach (SpatialElement room in allRooms)
                {
                    List<XYZ> concaveXyz = GetRoomCorners(room).Item1;
                    List<XYZ> convexXyz = GetRoomCorners(room).Item2;

                    // get the floor data
                    Floor floor = GetRoomBoundingFloor(allFloors, room);
                    PlanarFace bottomFace = GetFloorBottomFace(floor);  // need this
                    BoundingBoxUV faceBb = bottomFace.GetBoundingBox();
                    UV locationUv = faceBb.Max + faceBb.Min * 0.5;
                    XYZ refDir = bottomFace.ComputeNormal(locationUv).CrossProduct(XYZ.BasisZ); // need this

                    // get the symbol
                    FamilySymbol symbol = GetSymbol("Bimorph");

                    // set the options
                    Options opt = new Options();
                    opt.ComputeReferences = true;

                    // iterate through the points
                    foreach (XYZ point in concaveXyz)
                    {
                        doc.Create.NewFamilyInstance(bottomFace, point, refDir, symbol);
                    }
                }

                trans.Commit();
            }
        }

        public PlanarFace GetFloorBottomFace(Floor floor)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            GeometryElement gElement = floor.get_Geometry(opt);
            PlanarFace bottomFace = null;

            foreach (GeometryObject gObject in gElement)
            {
                if (gObject is Solid)
                {
                    Solid solid = (Solid)gObject;
                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace pFace = face as PlanarFace;
                        if (pFace != null && pFace.FaceNormal.Z < 0)
                        {
                            bottomFace = face as PlanarFace;
                            break;
                        }
                    }
                }
            }
            return bottomFace;
        }

        public FamilySymbol GetSymbol(string typeName)
        {
            return new FilteredElementCollector(doc).
                OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(f => f.Name.Contains(typeName))
                .First();
        }

        public Floor GetRoomBoundingFloor(List<Floor> floors, SpatialElement room)
        {
            foreach (Floor floor in floors)
            {
                //if (IsFloorRoomBounding(room, floor)) return floor; break;
                if (floor.IsFloorRoomBounding(doc, room)) return floor; break;
            }
            return null;
        }

        public double CalculateAngle(XYZ v1, XYZ v2)
        {
            return Math.Acos(v1.DotProduct(v2) / (v1.GetLength() * v2.GetLength())) * (180 / Math.PI);
        }

        public Tuple<List<XYZ>, List<XYZ>> GetRoomCorners(SpatialElement room)
        {
            List<XYZ> concaveXyz = new List<XYZ>();
            List<XYZ> convexXyz = new List<XYZ>();

            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<BoundarySegment> outerSegments = room.GetBoundarySegments(options).First();

            for (int i = 0; i < outerSegments.Count; i++)
            {
                int n;
                if (i == outerSegments.Count - 1) n = 0;
                else n = i + 1;

                XYZ evaluationPoint = outerSegments[i].GetCurve().GetEndPoint(1);
                XYZ prevPoint = outerSegments[i].GetCurve().GetEndPoint(0);
                XYZ nextPoint = outerSegments[n].GetCurve().GetEndPoint(1);

                double angle = CalculateAngle(evaluationPoint - prevPoint, nextPoint - evaluationPoint);
                bool concave = BimorphUtil.IsConcave(evaluationPoint - prevPoint, nextPoint - evaluationPoint);

                if (angle.AlmostEqualTo(90, 0.01) && concave) concaveXyz.Add(evaluationPoint);
                else if (angle.AlmostEqualTo(90, 0.01) && !concave) convexXyz.Add(evaluationPoint);
            }
            return new Tuple<List<XYZ>, List<XYZ>>(concaveXyz, convexXyz);
        }
    }
}
