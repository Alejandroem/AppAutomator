using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Threading;
using log4net;
using System.Configuration;
using System.Threading.Tasks;

namespace AppAutomator
{

    class Program
    {
        
        static async Task Main(string[] args)
        {

            BrowserRoutine browserRoutine = new BrowserRoutine();
            await  browserRoutine.runForConfiguredTimesAsync();
        }
    }
}
