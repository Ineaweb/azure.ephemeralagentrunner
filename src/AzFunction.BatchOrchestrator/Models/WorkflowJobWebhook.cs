using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchOrchestrator.Models
{
    public class WorkflowJobWebhook
    {
        public string action { get; set; }

        public WorkflowJobContent workflow_job { get; set; }

        public OrganisationContent organization { get; set; }

        public RepositoryContent repository { get; set; }

        // OS Used
        public string RunnerOSType {
            get
            {
                if (workflow_job.labels.Contains("ubuntu-18.04"))
                    return "ubuntu-18.04";
                if (workflow_job.labels.Contains("windows-latest"))
                    return "windows-latest";
                return "ubuntu-latest";
            }
        }

        public string RunnerPoolName { get; set; }

        // Type of Build Used
        public string RunnerBuildType { get; set; }

        public string ApiUrl
        {
            get
            {
                if (organization != null && organization.url != null)
                    return organization.url;
                if (repository != null && repository.url != null)
                    return repository.url;
                return "";
            }
        }

        public string RunnerUrl
        {
            get
            {
                if (organization != null && organization.url != null)
                    return $"https://github.com/{organization.login}";
                if (repository != null && repository.url != null)
                    return $"https://github.com/{repository.full_name}";
                return "";
            }
        }

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

    public class WorkflowJobContent
    {
        public string status { get; set; }

        public List<string> labels
        {
            get;
            set;
        } 
    }

    public class OrganisationContent
    {
        public string login { get; set; }

        public string url { get; set; }
    }

    public class RepositoryContent
    {
        public string url { get; set; }

        public string full_name { get; set; }
    }
}
