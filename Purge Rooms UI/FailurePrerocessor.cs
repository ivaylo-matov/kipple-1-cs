using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purge_Rooms_UI
{
    public class FailurePrerocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor fAccessor)
        {
            IList<FailureMessageAccessor> failMessages = fAccessor.GetFailureMessages();

            if (failMessages.Count() == 0)
            {
                return FailureProcessingResult.Continue;
            }
            else
            {
                foreach (FailureMessageAccessor failMessage in failMessages)
                {
                    FailureSeverity fSeverity = fAccessor.GetSeverity();
                    if (fSeverity == FailureSeverity.Warning)
                    {
                        fAccessor.DeleteWarning(failMessage);
                        return FailureProcessingResult.ProceedWithCommit;
                    }
                    else
                    {
                        fAccessor.ResolveFailure(failMessage);
                        return FailureProcessingResult.ProceedWithCommit;
                    }
                }
                return FailureProcessingResult.ProceedWithCommit;

                Console.WriteLine("something");
            }
        }
    }
}
