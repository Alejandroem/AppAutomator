using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Threading;
using log4net;


namespace AppAutomator
{

    class Program
    {
        
        static void Main(string[] args)
        {
            Logger.log.Info("Hello logging world!");
            Logger.log.Info("Hello logging world!");
            Logger.log.Info("Hello logging world!");
            Logger.log.Info("Hello logging world!");


            //WireGuard wireGuard = new WireGuard(new []{ "QA21", "QA22"});
            //WireGuard wireGuard = new WireGuard(new[] { "QA24", "QA25" });
            //Bot bot = new Bot();

            for (int i = 0; i < 3; i++) {
                //wireGuard.switchToNextNetwork();
                //bot.waitUntilVisibleAgain();
                //bot.clickContinue();
                //bot.waitUntilVisibleAgain();
                Thread.Sleep(5000);
            }
        }
    }
}
