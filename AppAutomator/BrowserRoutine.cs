using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace AppAutomator
{
    class BrowserRoutine
    {
        WireGuard wireGuard;
        Bot bot;

        public async Task runForConfiguredTimesAsync()
        {
            string[] networks = ConfigurationManager.AppSettings["networks"].Split(',');

            wireGuard = new WireGuard(networks);
            bot = new Bot();

            int runs = int.Parse(ConfigurationManager.AppSettings["runs"]);

            for (int i = 0; i < runs; i++)
            {
                await startBrowserRoutineAsync();
            }
        }

        async Task startBrowserRoutineAsync()
        {
            if(await wireGuard.performNetworkSwitchUntilConnectionItsValidAsync()) { 
                bot.doAFullRunAndWaitToTryAgain();
            }
        }


    }
}
