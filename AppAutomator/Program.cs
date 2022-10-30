using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Threading;
using log4net;
using System.Configuration;

namespace AppAutomator
{

    class Program
    {
        
        static void Main(string[] args)
        {

            BrowserRoutine browserRoutine = new BrowserRoutine();
            _ = browserRoutine.runForConfiguredTimesAsync();
        }
    }
}
