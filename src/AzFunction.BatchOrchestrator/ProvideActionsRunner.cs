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
    public class ProvideActionsRunner
    {
        private ILogger _logger;

        [FunctionName("ProvideActionsRunner")]
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
                var result = await RunInternal(JsonConvert.DeserializeObject<RunnerRequest>(jsonData));

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return new ConflictObjectResult(ex);
            }
        }

        internal async Task<IActionResult> RunInternal(RunnerRequest request)
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
                        await job.CommitAsync();
                    }

                    await batchClient.JobOperations.AddTaskAsync(job.Id, task);
                }

                result = new OkObjectResult(null);

            return result;
        }

        internal CloudTask InitializeCloudTask(RunnerRequest singleTask)
        {
            this._logger.LogInformation("Initializing CloudTask Task '{singleTaskName}'...", singleTask.TaskName);

            var cloudTask = new CloudTask(singleTask.TaskName, "")
            {
                ContainerSettings = new TaskContainerSettings($"{EnvironmentVariables.ContainerRegistryServer}/githubactions-runner:{singleTask.RunnerImageName}"),
                UserIdentity = new UserIdentity(new AutoUserSpecification(AutoUserScope.Task, ElevationLevel.Admin)),
                EnvironmentSettings = new List<EnvironmentSetting>()
                {
                    new EnvironmentSetting("ActionsRunner_URL", singleTask.GithubRepositoryUrl),
                    new EnvironmentSetting("ActionsRunner_TOKEN", singleTask.GithubRepositoryToken),
                    new EnvironmentSetting("ActionsRunner_POOL", singleTask.RunnerPoolName),
                    new EnvironmentSetting("ActionsRunner_DISPOSE", "true")
                }
            };

            return cloudTask;
        }
    }
}
