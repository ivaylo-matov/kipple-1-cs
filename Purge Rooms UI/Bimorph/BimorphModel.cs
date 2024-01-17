using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace Purge_Rooms_UI.Bimorph
{
    public class BimorphModel
    {
        public UIApplication UIApp { get; }         // read-only property to hold the UiApp
        public Document Doc { get; }                // read-only property to hold the document
        public BimorphModel(UIApplication uiApp)    // constructor
        {            
            Doc = uiApp.ActiveUIDocument.Document;        
        }

        public void MainCode()                      // processes below
        {
            // get all valid rooms and floors
            List<SpatialElement> allRooms = new FilteredElementCollector(Doc)
                .OfClass(typeof(SpatialElement))
                .WhereElementIsNotElementType()
                .Cast<SpatialElement>()
                .Where(x => x is Room)
                .Where(x => x.Area > 0)
            .ToList();

            List<Floor> allFloors = new FilteredElementCollector(Doc)
                .OfClass(typeof(Floor))
                .Cast<Floor>()
                .ToList();

            using (Transaction trans = new Transaction(Doc, "Place elements"))
            {
                trans.Start();

                // get list of mapped floors
                foreach (SpatialElement room in allRooms)
                {
                    List<XYZ> concaveXyz = GetRoomCorners(room).Item1;
                    List<XYZ> concaveVs = GetRoomCorners(room).Item2;

                    List<XYZ> convexXyz = GetRoomCorners(room).Item3;
                    List<XYZ> convexVs = GetRoomCorners(room).Item4;

                    // get the floor data
                    Floor floor = GetRoomBoundingFloor(allFloors, room);
                    PlanarFace bottomFace = GetFloorBottomFace(floor);  // need this
                    BoundingBoxUV faceBb = bottomFace.GetBoundingBox();
                    UV locationUv = faceBb.Max + faceBb.Min * 0.5;
                    XYZ refDir = bottomFace.ComputeNormal(locationUv).CrossProduct(XYZ.BasisZ); // need this

                    // get the symbols
                    FamilySymbol concaveSymbol = GetSymbol("Concave");
                    FamilySymbol convexSymbol = GetSymbol("Convex");

                    // set the options
                    Options opt = new Options();
                    opt.ComputeReferences = true;

                    // iterate through the points
                    if (concaveXyz.Count > 0 && concaveSymbol != null)
                    {
                        for (int i = 0; i < concaveXyz.Count; i++)
                        {
                            FamilyInstance instance = Doc.Create
                                .NewFamilyInstance(bottomFace, concaveXyz[i], concaveVs[1], concaveSymbol);
                            instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0);
                        }
                    }
                    if (convexXyz.Count > 0 && convexSymbol != null)
                    {
                        for (int i = 0; i < convexXyz.Count; i++)
                        {
                            FamilyInstance instance = Doc.Create
                                .NewFamilyInstance(bottomFace, convexXyz[i], convexVs[i], convexSymbol);
                            instance.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0);
                        }
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

        public FamilySymbol GetSymbol(string famKeyword)
        {
            FamilySymbol symbol = new FilteredElementCollector(Doc).
                OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(f => f.FamilyName.Contains(famKeyword))
                .First();

            if(!symbol.IsActive) symbol.Activate();
            return symbol;
        }

        public Floor GetRoomBoundingFloor(List<Floor> floors, SpatialElement room)
        {
            Floor boudingFloor = null;

            foreach (Floor floor in floors)
            {
                if (floor.IsFloorRoomBounding(Doc, room)) boudingFloor = floor;
            }
            return boudingFloor;
        }

        public double CalculateAngle(XYZ v1, XYZ v2)
        {
            return Math.Acos(v1.DotProduct(v2) / (v1.GetLength() * v2.GetLength())) * (180 / Math.PI);
        }

        public Tuple<List<XYZ>, List<XYZ>, List<XYZ>, List<XYZ>> GetRoomCorners(SpatialElement room)
        {
            List<XYZ> concaveXyz = new List<XYZ>();
            List<XYZ> convexXyz = new List<XYZ>();
            List<XYZ> concaveVs = new List<XYZ>();
            List<XYZ> convexVs = new List<XYZ>();

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
                XYZ v = prevPoint - evaluationPoint;    // works for convex only

                if (angle.AlmostEqualTo(90, 0.01) && concave)
                {
                    concaveVs.Add(v);
                    concaveXyz.Add(evaluationPoint);
                }
                else if (angle.AlmostEqualTo(90, 0.01) && !concave)
                {
                    convexVs.Add(v);
                    convexXyz.Add(evaluationPoint);
                }
            }
            return new Tuple<List<XYZ>, List<XYZ>, List<XYZ>, List<XYZ>>(concaveXyz, concaveVs, convexXyz, convexVs);
        }
    }
}
