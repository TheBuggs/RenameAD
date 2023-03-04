using System;
using System.Management;
using System.Management.Instrumentation;

namespace Rename2AD
{
    class Rename
    {
        static public bool Run(string username, string password)
        {

            using (ManagementObject wmiObject = new ManagementObject(new ManagementPath("Win32_ComputerSystem.Name='" + Environment.MachineName + "'")))
            {

                ManagementBaseObject inputArgs = wmiObject.GetMethodParameters("Rename");
                inputArgs["Name"] = Program.Hostname;
                inputArgs["Password"] = password;
                inputArgs["UserName"] = username;

                // THE MAIN PART
                ManagementBaseObject nameParams = wmiObject.InvokeMethod("Rename", inputArgs, null);

                if ((uint)(nameParams.Properties["ReturnValue"].Value) != 0)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
