using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purge_Rooms_UI
{
    [Transaction(TransactionMode.Manual)]
    internal class CheckFamilyNaming : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<Family> famFailed = new List<Family>();
            List<Family> famPassed = new List<Family>();

            using (Transaction t = new Transaction(doc, "Check family naming"))
            {
                t.Start();

                var allFamilies = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToList();
                foreach (Family fam in allFamilies)
                {
                    try
                    {
                        string[] codes = fam.Name.Split('-');
                        bool isNumberic = int.TryParse(codes[1], out int n);
                        if (isNumberic == true && codes.Count() >= 3)
                        {
                            famPassed.Add(fam);
                        }
                    }
                    catch
                    {
                        // do nothing
                    }

                }
                foreach (Family fam in allFamilies)
                {
                    if (!famPassed.Contains(fam) && !famFailed.Contains(fam))
                    {
                        famFailed.Add(fam);
                    }
                }

                string reportFams = "List of non-compliant family names:@";
                foreach (Family fam in famFailed)
                {
                    reportFams += fam.Name;
                    reportFams += "@";
                }

                reportFams = reportFams.Replace("@", System.Environment.NewLine);
                TaskDialog.Show("List of non-compliant family names", reportFams);

                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}
