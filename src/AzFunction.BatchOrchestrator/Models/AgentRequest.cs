using BatchOrchestrator.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchOrchestrator.Models
{

    public class AgentRequest
    {
        // OS Used
        public string AgentOSType { get; set; }

        // Type of Build Used
        public string AgentBuildType { get; set; }

        public string AgentImageName
        {
            get
            {
                string result = "";

                switch (AgentOSType)
                {
                    case "ubuntu-18.04":
                        result += "ubuntu-18.04-azdo";
                        break;
                    case "ubuntu-latest":
                        result += "ubuntu-20.04-azdo";
                        break;
                    case "windows-latest":
                        result += "windows-core-ltsc2019-azdo";
                        break;
                    default:
                        result += "ubuntu-20.04-azdo";
                        break;

                }

                if (!string.IsNullOrEmpty(AgentBuildType))
                {
                    result += $"-{AgentBuildType}";
                }

                return result;
            }
        }

        public string JobName
        {
            get
            {
                string result = "";

                switch (AgentOSType)
                {
                    case "ubuntu-18.04":
                        result += "AzDoLinux";
                        break;
                    case "ubuntu-latest":
                        result += "AzDoLinux";
                        break;
                    case "windows-latest":
                        result += "AzDOWindows";
                        break;
                    default:
                        result += "AzDoLinux";
                        break;

                }

                return result;
            }
        }

        public string TaskName
        {
            get
            {
                Guid guid = Guid.NewGuid();
                string base64 = Convert.ToBase64String(guid.ToByteArray()).Substring(0, 22).Replace("/", "_").Replace("+", "-");
                return $"RunAgent-{base64}";
            }
        }

        public string PoolName
        {
            get
            {
                string result = "";

                switch (AgentOSType)
                {
                    case "ubuntu-18.04":
                        result = "batchLinux";
                        break;
                    case "ubuntu-latest":
                        result = "batchLinux";
                        break;
                    case "windows-latest":
                        result = "batchWindows";
                        break;
                    default:
                        result = "batchLinux";
                        break;

                }

                return result;
            }
        }

        public string AzDoAgentPoolName
        {
            get
            {
                string result = "";

                switch (AgentOSType)
                {
                    case "ubuntu-18.04":
                        result = EnvironmentVariables.AzDOUbuntuPool;
                        break;
                    case "ubuntu-latest":
                        result = EnvironmentVariables.AzDOUbuntuPool;
                        break;
                    case "windows-latest":
                        result = EnvironmentVariables.AzDOWindowsPool;
                        break;
                    default:
                        result = EnvironmentVariables.AzDOUbuntuPool;
                        break;
                }

                return result;
            }
        }

    }
}
