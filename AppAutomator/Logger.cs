using System;
using System.Collections.Generic;
using System.Text;

namespace AppAutomator
{
    class Logger
    {
        public static readonly log4net.ILog log
       = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}
