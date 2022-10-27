using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Threading;

namespace AppAutomator
{

    class Program
    {
        static void Main(string[] args)
        {

            //WireGuard wireGuard = new WireGuard(new []{ "QA21", "QA22"});
            WireGuard wireGuard = new WireGuard(new[] { "QA24", "QA25" });
            Bot bot = new Bot();

            for(int i = 0; i < 3; i++) { 
                wireGuard.switchToNextNetwork();
                bot.clickContinue();
                bot.waitUntilVisibleAgain();
            }
        }
    }
}
