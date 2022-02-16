using BatchOrchestrator.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchOrchestrator.Models
{

    public class RunnerRequest
    {
        // OS Used
        public string RunnerOSType { get; set; }

        // Type of Build Used
        public string RunnerBuildType { get; set; }

        public string RunnerPoolName { get; set; }

        public string GithubRepositoryUrl { get; set; }

        public string GithubRepositoryToken { get; set; }

        public string RunnerImageName
        {
            get
            {
                string result = "";

                switch (RunnerOSType)
                {
                    case "ubuntu-18.04":
                        result += "ubuntu-18.04-actionsrunner";
                        break;
                    case "ubuntu-latest":
                        result += "ubuntu-20.04-actionsrunner";
                        break;
                    case "windows-latest":
                        result += "windows-core-ltsc2019-actionsrunner";
                        break;
                    default:
                        result += "ubuntu-20.04-actionsrunner";
                        break;

                }

                if (!string.IsNullOrEmpty(RunnerBuildType))
                {
                    result += $"-{RunnerBuildType}";
                }

                return result;
            }
        }

        public string JobName
        {
            get
            {
                string result = "";

                switch (RunnerOSType)
                {
                    case "ubuntu-18.04":
                        result += "RunnerLinux";
                        break;
                    case "ubuntu-latest":
                        result += "RunnerLinux";
                        break;
                    case "windows-latest":
                        result += "RunnerWindows";
                        break;
                    default:
                        result += "RunnerLinux";
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

                switch (RunnerOSType)
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
    }
}
