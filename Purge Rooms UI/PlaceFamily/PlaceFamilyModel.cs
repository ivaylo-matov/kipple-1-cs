using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using System;
using Autodesk.Revit.UI.Selection;

namespace Purge_Rooms_UI.PlaceFamily
{
    public class PlaceFamilyModel
    {
        public UIDocument UiDoc { get; set; }
        public Document Doc { get; set; }
    }
}
