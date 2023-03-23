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
        public static void ProcessFailuresEvents(object sender, FailuresProcessingEventArgs args)
        {
            FailuresAccessor fAccessor = args.GetFailuresAccessor();
            List<FailureMessageAccessor> fMessages = fAccessor.GetFailureMessages().ToList();
            foreach (FailureMessageAccessor fMessage in fMessages)
            {
                try
                {
                    fAccessor.DeleteWarning(fMessage);
                }
                catch { }
            }
        }
    }
}
