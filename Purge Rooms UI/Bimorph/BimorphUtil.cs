using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Purge_Rooms_UI.Bimorph
{
    static class BimorphUtil
    {
        public static View3D Def3DView(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault();
        }

        public static double MetersToFeet(this double value)
        {
            return value / 0.3048;
        }

        public static bool IsFloorRoomBounding(this Floor floor, Document doc, SpatialElement room)
        {
            View3D def3D = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault();

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

        public static bool IsConcave(XYZ v1, XYZ v2)
        {
            XYZ crossProduct = new XYZ(
            v1.Y * v2.Z - v1.Z * v2.Y,
            v1.Z * v2.X - v1.X * v2.Z,
            v1.X * v2.Y - v1.Y * v2.X);
            return crossProduct.Normalize().Z > 0;
        }

        public static bool AlmostEqualTo(this double value, double target, double tolerance)
        {
            return Math.Abs(value - target) <= tolerance;
        }
    }
}
