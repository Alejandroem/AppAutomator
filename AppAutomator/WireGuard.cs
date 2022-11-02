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
        public string Status { get; set; }
        public string NextQa { get; set; }
        public string ValidIp { get; set; }
        public string Message { get; set; }
    }

    class IpServerRquest
    {
        public String ip { get; set; }
        public String qa { get; set; }
        public String machine { get; set; }
    }

    class WireGuard
    {

        //TODO move to a config file
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

        public async Task<bool> performNetworkSwitchUntilConnectionItsValidAsync()
        {
            int ipRetries = int.Parse(ConfigurationManager.AppSettings["ipRetries"]);
            
            bool isValidIp = await isValidIpAsync();
            while (!isValidIp)
            {
                Logger.log.Info("Ip Valid Retry number: " + ipRetries);
                switchToNextNetwork();

                int internetRetries = int.Parse(ConfigurationManager.AppSettings["packagesRetries"]);
                
                bool hasInternet = await hasInternetAsync();
                while (!hasInternet)
                {
                    Logger.log.Info("Has Internet retries: " + internetRetries);
                    toggleCurrentNetwork();                    
                    internetRetries--;
                    if (internetRetries == 0)
                    {
                        break;
                    }
                    hasInternet = await hasInternetAsync();
                }


                ipRetries--;
                if (ipRetries == 0)
                {
                    break;
                }

                isValidIp = await isValidIpAsync();
            }
            return isValidIp;
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
            Logger.log.Info("Wating for 2 secs to test ip");
            Thread.Sleep(2000);
            Logger.log.Info("Testing valid ip");
            try
            {
                if (activeNetwork < 0)
                {
                    return false;
                }

                IpServerRquest ipServerRquest = new IpServerRquest();
                ipServerRquest.ip = getCurrentIP();
                ipServerRquest.qa = networks[activeNetwork];
                ipServerRquest.machine = ConfigurationManager.AppSettings["machine"];


                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(2);

                String apiKey = ConfigurationManager.AppSettings["apiKey"];
                client.DefaultRequestHeaders.Add("api-key", apiKey);

                String serverURL = ConfigurationManager.AppSettings["serverURL"];
                HttpResponseMessage response = await client.PostAsJsonAsync(serverURL + "/ipvalidate/validateip", ipServerRquest);

                response.EnsureSuccessStatusCode();
                IpServerResponse responseObject = await response.Content.ReadFromJsonAsync<IpServerResponse>();


                Logger.log.Info(responseObject.Message);
                Logger.log.Info(responseObject.ValidIp);
                Logger.log.Info(responseObject.NextQa);
                Logger.log.Info(responseObject.Status);
                if(responseObject.ValidIp == "yes")
                {
                    return true;
                }
                
            }
            catch (Exception e)
            {
                Logger.log.Info(e.Message);
                Logger.log.Info("Something went wrong while calling remote server to check ip");
            }
            return false;
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
                            Logger.log.Info("Network activated succesfully");
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
                    Logger.log.Info("Switching to: " + networks[networkToSwitchTo]);
                    wireGuardWindow.FocusNative();
                    wireGuardWindow.Focus();
                    ListBoxItem QAButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(networks[networkToSwitchTo])).AsListBoxItem();
                    QAButton.Select();
                    Thread.Sleep(1000);
                    wireGuardWindow.FocusNative();
                    wireGuardWindow.Focus();
                    Button activateButton = wireGuardWindow.FindFirstDescendant(cf => cf.ByText(ACTIVATE))?.AsButton();
                    activateButton.WaitUntilClickable();
                    activateButton.Click();

                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(500);
                        wireGuardWindow.FocusNative();
                        wireGuardWindow.Focus();
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

        public async Task<bool> hasInternetAsync()
        {
            Logger.log.Info("Wating for 2 secs to test packages");
            Thread.Sleep(2000);
            Logger.log.Info("Testing internet");
            try
            {

                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(2);
                HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://google.com/"));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Logger.log.Info("200 status code we have internet");
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.log.Info(e.Message);
            }
            Logger.log.Error("200 is not returning from the server");
            return false;
        }
    }
}
