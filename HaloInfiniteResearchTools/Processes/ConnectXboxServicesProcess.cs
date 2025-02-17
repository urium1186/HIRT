using LibHIRT.Grunt;
using LibHIRT.Utils;
using OpenSpartan.Grunt.Authentication;
using OpenSpartan.Grunt.Core;
using OpenSpartan.Grunt.Models;
using OpenSpartan.Grunt.Util;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HaloInfiniteResearchTools.Processes
{

    public class ConnectXboxServicesProcess : ProcessBase<ConnectXboxServicesResult>
    {
        HttpListener _httpListener = new HttpListener();

        bool do_not_stop = true;
        bool do_not_stop_listener = true;
        ConcurrentDictionary<string, bool> _stateLookup;
        Thread _responseThread;

        string ret_code = "";

        ConnectXboxServicesResult result;

        protected override Task OnInitializing()
        {
            if (_stateLookup == null)
                _stateLookup = new ConcurrentDictionary<string, bool>();
            else
                _stateLookup.Clear();

            return base.OnInitializing();
        }

        public override ConnectXboxServicesResult Result => result;

        protected async override Task OnExecuting()
        {
            _stateLookup = new ConcurrentDictionary<string, bool>();
            /**/
            try
            {
                button1_Click(this, null);
            }
            catch (Exception ex)
            {
                if (_responseThread != null && _responseThread.IsAlive)
                {
                    do_not_stop_listener = false;
                }
                throw ex;
            }

        }

        private void StarServerLocalHost()
        {
            if (!IsPortInUse(9090))
            {
                LogWriter.LogWrite("Starting server...");
                _httpListener.Prefixes.Add("http://localhost:9090/"); // add prefix "http://localhost:9090/"
                if (!_httpListener.IsListening)
                    try
                    {
                        _httpListener.Start(); // start server (Run application as Administrator!)
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                LogWriter.LogWrite("Server started.");
                if (_responseThread == null)
                    _responseThread = new Thread(ResponseThread);
                if (_responseThread.IsAlive)
                {
                    do_not_stop_listener = false;
                }
                _stateLookup["do_not_stop_listener"] = true;
                _responseThread.Start(); // start the response
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            XboxAuthenticationClient manager = new();
            var clientConfig = ConfigurationReader.ReadConfiguration<ClientConfiguration>(AppDomain.CurrentDomain.BaseDirectory + "Resources\\xboxservice\\client.json");
            var url = manager.GenerateAuthUrl(clientConfig.ClientId, clientConfig.RedirectUrl);

            HaloAuthenticationClient haloAuthClient = new();

            OAuthToken currentOAuthToken = null;

            var ticket = new XboxTicket();
            var haloTicket = new XboxTicket();
            var extendedTicket = new XboxTicket();
            var haloToken = new SpartanToken();
            string pathToketn = Utils.GetUserXboxTokenPath();
            if (System.IO.File.Exists(pathToketn))
            {
                LogWriter.LogWrite("Trying to use local tokens...");

                // If a local token file exists, load the file.
                currentOAuthToken = ConfigurationReader.ReadConfiguration<OAuthToken>(pathToketn);
            }
            else
            {
                currentOAuthToken = RequestNewToken(url, manager, clientConfig);
            }

            Task.Run(async () =>
            {
                ticket = await manager.RequestUserToken(currentOAuthToken.AccessToken);
                if (ticket == null)
                {
                    // There was a failure to obtain the user token, so likely we need to refresh.
                    currentOAuthToken = await manager.RefreshOAuthToken(
                        clientConfig.ClientId,
                        currentOAuthToken.RefreshToken,
                        clientConfig.RedirectUrl,
                        clientConfig.ClientSecret);

                    if (currentOAuthToken == null)
                    {
                        LogWriter.LogWrite("Could not get the token even with the refresh token.");
                        currentOAuthToken = RequestNewToken(url, manager, clientConfig);
                    }
                    ticket = await manager.RequestUserToken(currentOAuthToken.AccessToken);
                }
            }).GetAwaiter().GetResult();

            Task.Run(async () =>
            {
                haloTicket = await manager.RequestXstsToken(ticket.Token);
                if (haloTicket == null)
                {
                    currentOAuthToken = await manager.RefreshOAuthToken(
                        clientConfig.ClientId,
                        currentOAuthToken.RefreshToken,
                        clientConfig.RedirectUrl,
                        clientConfig.ClientSecret);

                    if (currentOAuthToken == null)
                    {
                        LogWriter.LogWrite("Could not get the token even with the refresh token.");
                        currentOAuthToken = RequestNewToken(url, manager, clientConfig);
                    }
                    ticket = await manager.RequestUserToken(currentOAuthToken.AccessToken);
                    haloTicket = await manager.RequestXstsToken(ticket.Token);
                }
            }).GetAwaiter().GetResult();

            Task.Run(async () =>
            {
                extendedTicket = await manager.RequestXstsToken(ticket.Token, false);
            }).GetAwaiter().GetResult();
            if (haloTicket is null)
            {
                if (_responseThread != null && _responseThread.IsAlive)
                {
                    do_not_stop_listener = false;
                }
                return;
            }

            Task.Run(async () =>
            {
                haloToken = await haloAuthClient.GetSpartanToken(haloTicket.Token);
                LogWriter.LogWrite("Your Halo token:");
                LogWriter.LogWrite(haloToken.Token);
            }).GetAwaiter().GetResult();

            HaloInfiniteClient client = new(haloToken.Token, extendedTicket.DisplayClaims.Xui[0].XUID);

            // Test getting the clearance for local execution.
            string localClearance = string.Empty;
            Task.Run(async () =>
            {
                var clearance = (await client.SettingsGetClearance("RETAIL", "UNUSED", "222249.22.06.08.1730-0")).Result;
                if (clearance != null)
                {
                    localClearance = clearance.FlightConfigurationId;
                    client.ClearanceToken = localClearance;
                    LogWriter.LogWrite($"Your clearance is {localClearance} and it's set in the client.");
                }
                else
                {
                    LogWriter.LogWrite("Could not obtain the clearance.");
                }
            }).GetAwaiter().GetResult();

            HaloInfiniteClientFix clientFix = new(client.SpartanToken, client.Xuid, client.ClearanceToken);
            result = new ConnectXboxServicesResult
            {
                Client = clientFix,
                ExtendedTicket = extendedTicket,
            };
            if (_responseThread != null && _responseThread.IsAlive)
            {
                do_not_stop_listener = false;
            }
            /*
            Task.Run(async () =>
            {
                //var example = await client.StatsGetMatchStats("21416434-4717-4966-9902-af7097469f74");
                var playerName = extendedTicket.DisplayClaims.Xui[0].Gamertag;
                var playerXUID = extendedTicket.DisplayClaims.Xui[0].XUID;
                var strPlayerXUID = "xuid(" + playerXUID + ")";

                var example = await client.EconomyPlayerOperations(strPlayerXUID);
                var armorCoresCustomization = await client.EconomyArmorCoresCustomization(strPlayerXUID);
                
                if (armorCoresCustomization.Result != null)
                {
                    string fileName = "armorCoresCustomization.json";
                    string jsonString = JsonSerializer.Serialize(armorCoresCustomization.Result);
                    File.WriteAllText(fileName, jsonString);
                }
                var bodyCustomization = await client.EconomySpartanBodyCustomization(strPlayerXUID);
                if (bodyCustomization.Result != null)
                {
                    string fileName = "bodyCustomization.json";
                    string jsonString = JsonSerializer.Serialize(bodyCustomization.Result);
                    File.WriteAllText(fileName, jsonString);
                }
                
                //if (_responseThread.IsAlive){
                    do_not_stop_listener = false;
                //}

                Debug.WriteLine("You have stats.");
            }).GetAwaiter().GetResult();
            */
        }

        public bool IsPortInUse(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpConnInfo in tcpConnInfoArray)
            {
                if (tcpConnInfo.LocalEndPoint.Port == port)
                {
                    return true;
                }
            }

            return false;
        }
        void ResponseThread()
        {
            while (_stateLookup.ContainsKey("do_not_stop_listener") && _stateLookup["do_not_stop_listener"])
            {
                HttpListenerContext context = _httpListener.GetContext(); // get a context
                                                                          // Now, you'll find the request URL in context.Request.Url


                if (context != null && context.Request != null)
                {
                    var body = context.Request.RawUrl;

                    Uri myUri = context.Request.Url;

                    ret_code = HttpUtility.ParseQueryString(myUri.Query).Get("code");

                    _stateLookup["do_not_stop"] = false;

                    byte[] _responseArray = Encoding.UTF8.GetBytes("<html><head><title>HIRT Localhost server -- port 9090</title></head>" +
                    "<body>HIRT Login Response Handled on <strong>Localhost server</strong> -- <em>port 9090!</em>. <strong>You have been authenticated. Please return to the HIRT application.</strong></body></html>"); // get the bytes to response
                    context.Response.OutputStream.Write(_responseArray, 0, _responseArray.Length); // write bytes to the output stream
                    context.Response.KeepAlive = false; // set the KeepAlive bool to false
                    context.Response.Close(); // close the connection
                    LogWriter.LogWrite("Respone given to a request." + ret_code);
                    _stateLookup["do_not_stop_listener"] = false;
                    _httpListener.Close();
                }

            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private OAuthToken RequestNewToken(string url, XboxAuthenticationClient manager, ClientConfiguration clientConfig)
        {
            LogWriter.LogWrite("Provide account authorization and grab the code from the URL:");
            LogWriter.LogWrite(url);
            _stateLookup["do_not_stop"] = true;
            StarServerLocalHost();
            OpenUrl(url);

            while (_stateLookup.ContainsKey("do_not_stop") && _stateLookup["do_not_stop"])
            { }
            LogWriter.LogWrite("Your code:");
            //var code = Console.ReadLine();
            var code = ret_code;

            var currentOAuthToken = new OAuthToken();

            // If no local token file exists, request a new set of tokens.
            Task.Run(async () =>
            {
                currentOAuthToken = await manager.RequestOAuthToken(clientConfig.ClientId, code, clientConfig.RedirectUrl, clientConfig.ClientSecret);
                if (currentOAuthToken != null)
                {
                    string pathToketn = Utils.GetUserXboxTokenPath();
                    var storeTokenResult = StoreTokens(currentOAuthToken, pathToketn);
                    if (storeTokenResult)
                    {
                        LogWriter.LogWrite("Stored the tokens locally.");
                    }
                    else
                    {
                        LogWriter.LogWrite("There was an issue storing tokens locally. A new token will be requested on the next run.");
                    }
                }
                else
                {
                    LogWriter.LogWrite("No token was obtained. There is no valid token to be used right now.");
                }
            }).GetAwaiter().GetResult();

            return currentOAuthToken;
        }

        private static bool StoreTokens(OAuthToken token, string path)
        {
            string json = JsonSerializer.Serialize(token);
            try
            {
                System.IO.File.WriteAllText(path, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
