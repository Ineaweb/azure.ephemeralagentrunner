using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Protocol;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BatchOrchestrator.Helpers
{
    public static class BatchHelpers
    {
        /// <summary>
        /// custom validation rules for batch Job or Task name 
        /// </summary>
        private static Regex batchNameCustomValidation = new Regex("^[a-z][a-z0-9-]+$");

        public static BatchClient CreateBatchClient()
        {
            if (string.IsNullOrWhiteSpace(EnvironmentVariables.BatchAccountUrl) || string.IsNullOrWhiteSpace(EnvironmentVariables.BatchAccountName) || string.IsNullOrWhiteSpace(EnvironmentVariables.BatchAccountKey))
            {
                throw new InvalidOperationException($"BathcAccount parameters are not set!");
            }
            var cred = new BatchSharedKeyCredentials(EnvironmentVariables.BatchAccountUrl, EnvironmentVariables.BatchAccountName, EnvironmentVariables.BatchAccountKey);
            return BatchClient.Open(cred);
        }
    }
}
