using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AppAutomator
{
    class WireGuard
    {
        /* TODO
         const String ACTIVATE = "Activate";
        const String DEACTIVATE = "Deactivate";
        */
        const String ACTIVATE = "Activar";
        const String DEACTIVATE = "Desactivar";
        Application wireGuardApp;

        String[] networks;
        int activeNetwork = -1;

        public WireGuard(string[] networks)
        {
            this.networks = networks;
            findWireGuardApp();
        }

        public void findWireGuardApp()
        {
            FlaUI.Core.Application.Launch("WireGuard.exe");
            Process[] wireGuardProcess = new Process[0];
            while (wireGuardProcess.Length == 0)
            {
                Console.WriteLine("I'm fishing for the process");
                wireGuardProcess = Process.GetProcessesByName("WireGuard");
                Thread.Sleep(1000);

            }
            wireGuardApp = FlaUI.Core.Application.Attach(wireGuardProcess[1].Id);
        }

        public bool appHasInitialized()
        {
            return this.wireGuardApp != null;
        }

        public bool hasActiveNetwork()
        {
            return this.activeNetwork > 0;
        }

        public bool switchToNextNetwork()
        {
            try { 
            int networkToSwitchTo = this.activeNetwork;
            if (networkToSwitchTo + 1 >= networks.Length)
            {
                networkToSwitchTo = 0;
            } else { 
                networkToSwitchTo++;
            }

            using (UIA3Automation wireGuardAutomation = new UIA3Automation())
            {
                Window wireGuardWindow = wireGuardApp.GetMainWindow(wireGuardAutomation);
                wireGuardWindow.FocusNative();
                wireGuardWindow.Focus();

                    //Desactivate if it's already activated
                Button initialDeactivateButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(DEACTIVATE))?.AsButton();
                if (initialDeactivateButton != null)
                {
                    initialDeactivateButton.Click();

                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(500);
                        Button activateFlag = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(ACTIVATE))?.AsButton();
                        if (activateFlag != null)
                        {
                            Console.WriteLine("Network deactivated succesfully");
                            break;
                        }
                    }

                    }

                ListBoxItem QAButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(networks[networkToSwitchTo])).AsListBoxItem();
                QAButton.Select();
                Thread.Sleep(1000);
                Button activateButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(ACTIVATE))?.AsButton();                
                activateButton.WaitUntilClickable();
                activateButton.Click();

                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(500);
                    Button deactivateButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(DEACTIVATE))?.AsButton();
                    if(deactivateButton != null)
                    {
                        Console.WriteLine("Network activated succesfully");
                        break; 
                    }
                }
                //Here goes making sure that we see the deactivate on the pannel
                //I also need to ensure that my ip is valid
            }

            //Do this only when the process succed
            this.activeNetwork = networkToSwitchTo;
            return true;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }
    }
}
