using System;
using System.Configuration;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace Rename2AD
{
    static class Program
    {

        public static string Domain = ReadSetting("Domain");
        public static string DomainOU = ReadSetting("DomainOU");
        public static string URL = ReadSetting("URL");
        public static string ApiKey = ReadSetting("ApiKey");
        public static string Hostname = String.Empty;
        public static bool ValidName = false;
        public static Hostname RealHostname = new Hostname();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Security.Principal.WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Verb = "runas";
                try
                {
                    Process p = Process.Start(startInfo);
                    Application.Exit();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show("This utility requires elevated priviledges to complete correctly.", "Error: UAC Authorisation Required", MessageBoxButtons.OK);
                    return;
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Form head = new Head();
                Application.Run(head);

                if (ValidName)
                {
                    head.Close();
                    Form l = new Login();
                    l.Focus();
                    Application.Run(l);
                }
            }
        }

        static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                return String.Empty;
            }
        }
    }
}
