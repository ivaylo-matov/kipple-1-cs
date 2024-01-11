using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Purge_Rooms_UI.Bimorph
{
    

    /// <summary>
    /// Interaction logic for BimorpfView.xaml
    /// </summary>
    public partial class BimorphView : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }

        public BimorphView(UIDocument UiDoc)
        {            
            uidoc = UiDoc;
            doc = uidoc.Document;

            InitializeComponent();
        }

        private void DoStuff(object sender, RoutedEventArgs e)
        {
            MainCode(doc);
            Close();
        }

        private void MainCode(Document doc)
        {
            ////Floor floor = new FilteredElementCollector(doc).OfClass(typeof(Floor)).Cast<Floor>().FirstOrDefault();
            ////// bottom reference
            ////PlanarFace bottomFace = GetFloorBottomFace(floor);

            ////// get the center of the wallFace
            ////BoundingBoxUV faceBb = bottomFace.GetBoundingBox();
            ////UV locationUv = faceBb.Max + faceBb.Min * 0.5;
            ////XYZ locationXyz = bottomFace.Evaluate((faceBb.Max + faceBb.Min) * 0.5);
            ////XYZ refDir = bottomFace.ComputeNormal(locationUv).CrossProduct(XYZ.BasisZ);

            ////// family symbol
            ////FamilySymbol symbol = GetSymbol("Bimorph");
            ////// create family instance
            ////Options opt = new Options();
            ////opt.ComputeReferences = true;
            ////doc.Create.NewFamilyInstance(bottomFace, locationXyz, refDir, symbol);



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
        }

        

        private PlanarFace GetFloorBottomFace(Floor floor)
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

        private FamilySymbol GetSymbol(string typeName)
        {
            return new FilteredElementCollector(doc).
                OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(f => f.Name.Contains(typeName))
                .First();
        }

        public double MatersToFeet(double value)
        {
            return value / 0.3048; 
        }

        private Floor GetRoomBoundingFloor(List<Floor> floors, SpatialElement room)
        {
            foreach (Floor floor in floors)
            {
                if (IsFloorRoomBounding(room, floor)) return floor; break;
            }
            return null;
        }

        private bool IsFloorRoomBounding(SpatialElement room, Floor floor)
        {
            View3D def3D = new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault();
            ElementClassFilter filter = new ElementClassFilter(typeof(Floor));

            LocationPoint location = room.Location as LocationPoint;
            XYZ center = location.Point;

            ReferenceIntersector refIntersector = new ReferenceIntersector(filter, FindReferenceTarget.Face, def3D);
            ReferenceWithContext refWithContext = refIntersector.FindNearest(center, XYZ.BasisZ);
            if (refWithContext != null)
            {
                Reference floorRef = refWithContext.GetReference();
                ElementId floorId = floorRef.ElementId;
                if (floorId.IntegerValue == floor.Id.IntegerValue) return true;
            }
            return false;
        }

        private double CalculateAngle(XYZ v1, XYZ v2)
        {
            return Math.Acos(v1.DotProduct(v2) / (v1.GetLength() * v2.GetLength())) * (180 / Math.PI);
        }

        private bool IsConcave(XYZ v1, XYZ v2)
        {
            XYZ crossProduct = new XYZ(
            v1.Y * v2.Z - v1.Z * v2.Y,
            v1.Z * v2.X - v1.X * v2.Z,
            v1.X * v2.Y - v1.Y * v2.X);
            return crossProduct.Normalize().Z > 0;
        }

        private bool AlmostEqualTo(double value, double target, double tolerance)
        {
            return Math.Abs(value - target) <= tolerance;
        }

        private Tuple<List<XYZ>, List<XYZ>> GetRoomCorners(SpatialElement room)
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

                XYZ evaluationPoint   = outerSegments[i].GetCurve().GetEndPoint(1);
                XYZ prevPoint = outerSegments[i].GetCurve().GetEndPoint(0);
                XYZ nextPoint = outerSegments[n].GetCurve().GetEndPoint(1);

                double angle = CalculateAngle(evaluationPoint - prevPoint, nextPoint - evaluationPoint);
                bool concave = IsConcave(evaluationPoint - prevPoint, nextPoint - evaluationPoint);  

                if (AlmostEqualTo(angle, 90, 0.01) && concave) concaveXyz.Add(evaluationPoint);
                else if (AlmostEqualTo(angle, 90, 0.01) && !concave) convexXyz.Add(evaluationPoint);
            }
            return new Tuple<List<XYZ>, List<XYZ>> (concaveXyz, convexXyz);
        }
    }
}
