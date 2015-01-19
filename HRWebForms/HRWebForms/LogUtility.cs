using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;


namespace HRWebForms
{
    static class LogUtility
    {
        public static void LogError(string category, string messageFormat, params object[] formatArgs)
        {
            SPDiagnosticsService diagSvc = SPDiagnosticsService.Local;
            diagSvc.WriteTrace(0, // custom trace id
                                new SPDiagnosticsCategory(category, TraceSeverity.High, EventSeverity.Error), // create a category
                                TraceSeverity.High, // set the logging level of this record
                                messageFormat, // custom message
                                formatArgs // parameters to message
                                );
        }
    }

}
