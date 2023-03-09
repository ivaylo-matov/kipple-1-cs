using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purge_Rooms_UI
{
    public class FailurePreprocessor_Event
    {
        public static void TestProcessor(object sender, FailuresProcessingEventArgs e)
        {
            Console.WriteLine("Shit!");
        }

        public static void ProcessFailures_Events(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor fas = e.GetFailuresAccessor();
            List<FailureMessageAccessor> fma = fas.GetFailureMessages().ToList();
            List<ElementId> del = new List<ElementId>();
            int errors = 0;
            foreach (FailureMessageAccessor fa in fma)
            {
                try
                {
                    if (fa.GetSeverity() == FailureSeverity.Error)
                    {
                        if (fas.IsFailureResolutionPermitted(fa, FailureResolutionType.DeleteElements))
                        {
                            fa.SetCurrentResolutionType(FailureResolutionType.DeleteElements);
                            fas.ResolveFailure(fa);
                            errors++;
                            continue;
                        }
                    }
                    else if (fa.GetSeverity() == FailureSeverity.Warning)
                    {
                        foreach (ElementId f in fa.GetAdditionalElementIds())
                        {
                            if (!del.Contains(f))
                            {
                                del.Add(f);
                            }
                        }
                        fas.DeleteWarning(fa);
                        if (del.Count > 0)
                        {
                            fas.DeleteElements(del);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Failure Accessor", ex.Message + "\n" + ex.ToString());
                    continue;
                }
            }
            if (errors > 0)
            {
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            }
        }
    }
}
