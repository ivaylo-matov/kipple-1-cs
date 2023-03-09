using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    public class PurgeFillPatternsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                PurgeFillPatternsView window = new PurgeFillPatternsView(uidoc);
                window.chkUnusedPatterns.Content = $"Purge unused fill patterns ({GetUnusedFillPatternIds(doc).Count().ToString()})";
                window.chkCADPat.Content = $"Purge imported CAD fill patterns ({GetCADFillPatternIds(doc).Count().ToString()})";
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static List<Element> GetAllFillPatterns(Document doc)
        {
            var allFillPatterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToList();
            return allFillPatterns;
        }
        public static List<ElementId> GetUsedFillPattenIds(Document doc)
        {
            List<ElementId> usedFillPatternIds = new List<ElementId>();
            try
            {
                var allFrTypes = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).ToList();
                foreach (FilledRegionType type in allFrTypes)
                {
                    if (usedFillPatternIds.Contains(type.BackgroundPatternId) == false)
                    {
                        usedFillPatternIds.Add(type.BackgroundPatternId);
                    }
                    if (usedFillPatternIds.Contains(type.ForegroundPatternId) == false)
                    {
                        usedFillPatternIds.Add(type.ForegroundPatternId);
                    }
                }
            }
            catch{ }
            return usedFillPatternIds;
        }
        public static List<ElementId> GetUnusedFillPatternIds(Document doc)
        {
            List<ElementId> unusedFillPatternIds = new List<ElementId>();
            try
            {
                List<ElementId> usedFillPatternIds = GetUsedFillPattenIds(doc);
                List<Element> allFillPatterns = GetAllFillPatterns(doc);
                foreach (Element pat in allFillPatterns)
                {
                    if (!usedFillPatternIds.Contains(pat.Id))
                    {
                        unusedFillPatternIds.Add(pat.Id);
                    }
                }
            }
            catch { }
            return unusedFillPatternIds;
        }
        public static List<ElementId> GetCADFillPatternIds(Document doc)
        {
            List<ElementId> CADFillPatternIds = new List<ElementId>();
            try
            {
                List<Element> allFillPatterns = GetAllFillPatterns(doc);
                foreach (Element pat in allFillPatterns)
                {
                    if (pat.Name.Contains("dwg") || pat.Name.Contains("IMPORT"))
                    {
                        CADFillPatternIds.Add(pat.Id);
                    }
                }
            }
            catch { }
            return CADFillPatternIds;
        }
    }
}