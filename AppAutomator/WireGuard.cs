using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppAutomator
{

    class IpServerResponse
    {
        public string status;
        public string nextqa;
        public string validip;
        public string message;
    }

    class WireGuard
    {
        /*
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
                Logger.log.Info("I'm fishing for the process");
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

        public async Task performNetworkSwitchUntilConnectionItsValidAsync()
        {

            Logger.log.Info("Starting to change network");
            int ipRetries = 10;
            while (!await isValidIpAsync())
            {
                Logger.log.Info("Ip Valid Retry number: " + ipRetries);

                switchToNextNetwork();

                Logger.log.Info("Wating for 5 secs to test packages");
                Thread.Sleep(5000);
                int internetRetries = 10;
                while (!hasInternet())
                {
                    Logger.log.Info("Has Internet retries: " + internetRetries);
                    toggleCurrentNetwork();
                    Logger.log.Info("Wating for 5 secs to test packages");
                    Thread.Sleep(5000);
                    internetRetries--;
                    if (internetRetries == 0)
                    {
                        break;
                    }
                }


                ipRetries--;
                if (ipRetries == 0)
                {
                    break;
                }
            }
        }


        public string getCurrentIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            Logger.log.Error("No network adapters with an IPv4 address in the system!");
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public async Task<bool> isValidIpAsync()
        {
            //TODO validate if current network it's -1
            if (activeNetwork < 0)
            {
                return false;
            }
            String ip = getCurrentIP();
            String machine = ConfigurationManager.AppSettings["machine"];
            Dictionary<string, string> values = new Dictionary<string, string>
                {
                    { "ip", ip },
                    { "qa", networks[activeNetwork] },
                    { "machine", machine }
                };


            FormUrlEncodedContent content = new FormUrlEncodedContent(values);


            HttpClient client = new HttpClient();

            String apiKey = ConfigurationManager.AppSettings["apiKey"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("api-key", apiKey);


            String serverURL = ConfigurationManager.AppSettings["serverURL"];
            HttpResponseMessage response = await client.PostAsJsonAsync(serverURL + "/ipvalidate/ValidateIP", content);

            response.EnsureSuccessStatusCode();
            IpServerResponse responseObject = await response.Content.ReadFromJsonAsync<IpServerResponse>();

            Logger.log.Info(responseObject.message);
            Logger.log.Info(responseObject.validip);
            Logger.log.Info(responseObject.nextqa);
            Logger.log.Info(responseObject.status);

            return true;
        }

        public bool toggleCurrentNetwork()
        {
            Logger.log.Info("Trying to toggle current network");
            try
            {
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
                                Logger.log.Info("Network deactivated succesfully");
                                break;
                            }
                        }

                    }
                    Thread.Sleep(1000);
                    Button activateButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(ACTIVATE))?.AsButton();
                    activateButton.WaitUntilClickable();
                    activateButton.Click();

                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(500);
                        Button deactivateButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(DEACTIVATE))?.AsButton();
                        if (deactivateButton != null)
                        {
                            Logger.log.Info("Network toggled succesfully");
                            break;
                        }
                    }
                    //Here goes making sure that we see the deactivate on the pannel
                    //I also need to ensure that my ip is valid
                }

                //Do this only when the process succed
                Thread.Sleep(2000);
                Logger.log.Info("Network toggled succesfully");
                return true;
            }
            catch (Exception e)
            {
                Logger.log.Info(e.Message);
            }
            Logger.log.Info("Something went wrong while trying to toggle the network");
            return false;
        }

        public bool switchToNextNetwork()
        {
            Logger.log.Info("Trying to switch network");
            try
            {
                int networkToSwitchTo = this.activeNetwork;
                if (networkToSwitchTo + 1 >= networks.Length)
                {
                    networkToSwitchTo = 0;
                }
                else
                {
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
                                Logger.log.Info("Network deactivated succesfully");
                                break;
                            }
                        }

                    }
                    Logger.log.Error("Switching to: " + networks[networkToSwitchTo]);
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
                        if (deactivateButton != null)
                        {
                            Logger.log.Info("Network activated succesfully");
                            break;
                        }
                    }
                    //Here goes making sure that we see the deactivate on the pannel
                    //I also need to ensure that my ip is valid
                }

                //Do this only when the process succed
                this.activeNetwork = networkToSwitchTo;
                Thread.Sleep(2000);
                Logger.log.Info("Successfully changed network");
                return true;
            }
            catch (Exception e)
            {
                Logger.log.Error(e.Message);
            }
            Logger.log.Error("Something went wrong while trying to switch network");
            return false;
        }

        public bool hasInternet()
        {
            try
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send("https://www.google.com/", 5000);
                if (reply != null)
                {
                    Logger.log.Info("Status :  " + reply.Status + " \n Time : " + reply.RoundtripTime.ToString() + " \n Address : " + reply.Address);

                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }

                }
            }
            catch(Exception e)
            {
                Logger.log.Info(e.Message);
                Logger.log.Info("ERROR: You have Some TIMEOUT issue");
            }
            return false;
        }
    }
}
