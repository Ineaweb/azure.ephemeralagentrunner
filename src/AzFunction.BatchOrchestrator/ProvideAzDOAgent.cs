using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;
using Newtonsoft.Json;
using BatchOrchestrator.Models;
using BatchOrchestrator.Helpers;
using System.Collections.Generic;

namespace BatchOrchestrator
{
    public class ProvideAzDOAgent
    {
        private ILogger _logger;

        [FunctionName("ProvideAzDOAgent")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                _logger = log;
                string jsonData = "";
                req.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(req.Body))
                {
                    jsonData = await reader.ReadToEndAsync();
                }
                var result = await RunInternal(JsonConvert.DeserializeObject<AgentRequest>(jsonData));

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return new ConflictObjectResult(ex);
            }
        }

        internal async Task<IActionResult> RunInternal(AgentRequest request)
        {
            var result = default(IActionResult);
                CloudTask task = InitializeCloudTask(request);

                using (var batchClient = BatchHelpers.CreateBatchClient())
                {
                    CloudJob job = null;

                    try
                    {
                        job = await batchClient.JobOperations.GetJobAsync(request.JobName);
                    }
                    catch 
                    {
                        job = batchClient.JobOperations.CreateJob();
                        job.PoolInformation = new PoolInformation() { PoolId = request.PoolName };
                        job.Id = request.JobName;
                        job.OnAllTasksComplete = OnAllTasksComplete.NoAction;
                        job.OnTaskFailure = OnTaskFailure.NoAction;
                        job.CommonEnvironmentSettings = new List<EnvironmentSetting>()
                            {
                                new EnvironmentSetting("AZDO_URL", EnvironmentVariables.AzDOUrl),
                                new EnvironmentSetting("AZDO_TOKEN", EnvironmentVariables.AzDOToken),
                                new EnvironmentSetting("AZDO_POOL", request.AzDoAgentPoolName),
                                new EnvironmentSetting("AZDO_AGENT_DISPOSE", "true")
                            };
                        await job.CommitAsync();
                    }

                    await batchClient.JobOperations.AddTaskAsync(job.Id, task);
                }

                result = new OkObjectResult(null);

            return result;
        }

        internal CloudTask InitializeCloudTask(AgentRequest singleTask)
        {
            this._logger.LogInformation("Initializing CloudTask Task '{singleTaskName}'...", singleTask.TaskName);

            var cloudTask = new CloudTask(singleTask.TaskName, "")
            {
                ContainerSettings = new TaskContainerSettings($"{EnvironmentVariables.ContainerRegistryServer}/azdo-agent:{singleTask.AgentImageName}"),
                UserIdentity = new UserIdentity(new AutoUserSpecification(AutoUserScope.Task, ElevationLevel.Admin))
            };

            return cloudTask;
        }
    }
}
