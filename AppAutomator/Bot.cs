using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Configuration;
using System.Threading;

namespace AppAutomator
{
    class Bot
    {
        //TODO move to a config file
        /*
        const String CONTINUE ="Continue";
        const String TRY_AGAIN = "Try again";
        const String CANCEL = "Cancel";
        */
        const String CONTINUE = "Omitir";
        const String TRY_AGAIN = "Reintentar";
        const String CANCEL = "Abortar";
        
        Application botApp;
        public Bot()
        {
            String botPath = ConfigurationManager.AppSettings["botPath"];
            botApp = FlaUI.Core.Application.Launch(botPath);

        }

        public void doAFullRunAndWaitToTryAgain()
        {
            waitUntilVisibleAgain();
            clickContinue();
            waitUntilVisibleAgain();
        }

        public void waitUntilVisibleAgain()
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                Window window = botApp.GetMainWindow(automation);

                while (!window.IsAvailable)
                {
                    Console.WriteLine("Waiting for windows to have focus");
                    Thread.Sleep(1000);
                }
                Console.WriteLine("Window now has focus");
            }
        }

        public void clickContinue()
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                Window window = botApp.GetMainWindow(automation);
                window.FocusNative();
                window.Focus();
                Console.WriteLine(window.Title);
                while(window.FindFirstDescendant(cf => cf.ByText(CONTINUE)) == null)
                {
                    Console.WriteLine("Waiting for continue");
                    Thread.Sleep(1000);
                }
                AutomationElement continueButton = window.FindFirstDescendant(cf => cf.ByText(CONTINUE))?.AsButton();
                continueButton.WaitUntilClickable();
                continueButton?.DoubleClick();
                Console.WriteLine("Clicked continue");
            }
        }

        public void clickTryAgain()
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                Window window = botApp.GetMainWindow(automation);
                window.FocusNative();
                Console.WriteLine(window.Title);
                AutomationElement tryAgainButton = window.FindFirstDescendant(cf => cf.ByText(TRY_AGAIN))?.AsButton();
                tryAgainButton.WaitUntilClickable();
                tryAgainButton?.Click();
                Console.WriteLine("Clicked try again");
            }
        }

        public void clickCancel()
        {
            using (UIA3Automation automation = new UIA3Automation())
            {
                Window window = botApp.GetMainWindow(automation);
                window.FocusNative();
                Console.WriteLine(window.Title);
                AutomationElement cancelButton = window.FindFirstDescendant(cf => cf.ByText(CANCEL))?.AsButton();
                cancelButton.WaitUntilClickable();
                cancelButton?.Click();
                Console.WriteLine("Clicked cancel");
            }
        }

    }
}
