using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Rename2AD
{
    class Http
    {
        private string newName;
        private string macAddress;
        private string uuid;
        private string user;
        private string key;
        private string ip;

        public Http(string newName, string username)
        {

            this.ip = this.GetLocalIPAddress();

            if (this.ip != String.Empty)
            {
                this.macAddress = this.GetLocalMACAddress(this.ip);
            }

            this.newName = newName;
            this.uuid = (this.GetSystemUUID()).Trim();
            this.user = username;
            this.key = "00000000000000000000000000000000000000";
            
        }

        public string NewName
        {
            get { return newName; }
        }

        public string MacAddress
        {
            get { return macAddress; }
        }

        public string Uuid
        {
            get { return uuid; }
        }

        public string User
        {
            get { return user; }
        }

        public string Ip
        {
            get { return ip; }
        }

        public string sendJSON()
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Program.URL + "/script-joinad-computer-log");

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Timeout = 5000;
            httpWebRequest.ReadWriteTimeout = 5000;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"key\":\"" + (this.key).ToString() + "\"," +
                                "\"new\":\"" + this.newName + "\"," +
                                "\"mac\":\"" + this.macAddress + "\"," +
                                "\"uuid\":\"" + this.uuid + "\"," +
                                "\"user\":\"" + this.user + "\"}";
                streamWriter.Write(json);
            }

            try
            {
                var httpResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();

                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result.ToString();
                }

            }
            catch (WebException webEx)
            {
                webEx.Response.Close();
            }

            return String.Empty;
        }

        private string GetLocalMACAddress(string ipAddress)
        {
            string macAddresses = string.Empty;

            // grab all online interfaces
            var query = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up && // only grabbing what's online
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(_ => new
                {
                    PhysicalAddress = _.GetPhysicalAddress(),
                    IPProperties = _.GetIPProperties(),
                });

            // grab the first interface that has a unicast address that matches your search string
            var mac = query
                .Where(q => q.IPProperties.UnicastAddresses
                    .Any(ua => ua.Address.ToString() == ipAddress))
                .FirstOrDefault()
                .PhysicalAddress;

            // return the mac address with formatting (eg "00-00-00-00-00-00")
            return String.Join(":", mac.GetAddressBytes().Select(b => b.ToString("X2")));
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return String.Empty;
            //throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private string GetSystemUUID()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "CMD.exe";
            startInfo.Arguments = "/C wmic csproduct get UUID";
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            
            string output = process.StandardOutput.ReadToEnd();
            output = (output.Trim()).Replace("UUID", "");
            return output.Trim();
        }
    }
}
