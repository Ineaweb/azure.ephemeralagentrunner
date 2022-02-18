using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchOrchestrator.Helpers
{
    public static class EnvironmentVariables
    {

        public static readonly string BatchAccountUrl = $"https://{Environment.GetEnvironmentVariable(nameof(BatchAccountUrl), EnvironmentVariableTarget.Process)}";

        public static readonly string BatchAccountName = Environment.GetEnvironmentVariable(nameof(BatchAccountName), EnvironmentVariableTarget.Process);

        public static readonly string BatchAccountKey = Environment.GetEnvironmentVariable(nameof(BatchAccountKey), EnvironmentVariableTarget.Process);

        public static readonly string ContainerRegistryServer = Environment.GetEnvironmentVariable(nameof(ContainerRegistryServer), EnvironmentVariableTarget.Process);

        public static readonly string AzDOUrl = Environment.GetEnvironmentVariable(nameof(AzDOUrl), EnvironmentVariableTarget.Process);

        public static readonly string AzDOToken = Environment.GetEnvironmentVariable(nameof(AzDOToken), EnvironmentVariableTarget.Process);

        public static readonly string AzDOUbuntuPool = Environment.GetEnvironmentVariable(nameof(AzDOUbuntuPool), EnvironmentVariableTarget.Process);

        public static readonly string AzDOWindowsPool = Environment.GetEnvironmentVariable(nameof(AzDOWindowsPool), EnvironmentVariableTarget.Process);

        public static readonly string GithubToken = Environment.GetEnvironmentVariable(nameof(GithubToken), EnvironmentVariableTarget.Process);
    }
}
