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
using RestSharp;

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

                var webhookData = JsonConvert.DeserializeObject<WorkflowJobWebhook>(jsonData);
                if (webhookData != null 
                    && webhookData.action == "queued"
                    && webhookData.workflow_job.status == "queued"
                    && webhookData.workflow_job.labels.Contains("self-hosted"))
                {
                    var result = await RunInternal(webhookData);

                    return await Task.FromResult(result);
                }
                return await Task.FromResult(default(IActionResult));
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                return new ConflictObjectResult(ex);
            }
        }

        internal async Task<string> GetRunnerToken(WorkflowJobWebhook request)
        {
            var client = new RestClient(request.ApiUrl);
            var rqst = new RestRequest("/actions/runners/registration-token", Method.Post);
            rqst.AddHeader("Accept", "application/vnd.github.v3+json");
            rqst.AddHeader("Authorization", $"token {EnvironmentVariables.GithubToken}");
            var queryResult = await client.ExecutePostAsync<RunnerTokenResponse>(rqst);

            return queryResult.IsSuccessful ? queryResult.Data.token : null;
        }

        internal async Task<IActionResult> RunInternal(WorkflowJobWebhook request)
        {
            var result = default(IActionResult);
            CloudTask task = await InitializeCloudTask(request);

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

        internal async Task<CloudTask> InitializeCloudTask(WorkflowJobWebhook singleTask)
        {
            this._logger.LogInformation("Initializing CloudTask Task '{singleTaskName}'...", singleTask.TaskName);

            var cloudTask = new CloudTask(singleTask.TaskName, "")
            {
                ContainerSettings = new TaskContainerSettings($"{EnvironmentVariables.ContainerRegistryServer}/githubactions-runner:{singleTask.RunnerImageName}"),
                UserIdentity = new UserIdentity(new AutoUserSpecification(AutoUserScope.Task, ElevationLevel.Admin)),
                EnvironmentSettings = new List<EnvironmentSetting>()
                {
                    new EnvironmentSetting("ActionsRunner_URL", singleTask.RunnerUrl),
                    new EnvironmentSetting("ActionsRunner_TOKEN", await GetRunnerToken(singleTask)),
                    new EnvironmentSetting("ActionsRunner_LABELS", string.Join(",",singleTask.workflow_job.labels)),
                    new EnvironmentSetting("ActionsRunner_DISPOSE", "true")
                }
            };

            //if (!string.IsNullOrEmpty(singleTask.RunnerPoolName))
            //{
            //    cloudTask.EnvironmentSettings.Add(new EnvironmentSetting("ActionsRunner_POOL", singleTask.RunnerPoolName));
            //}
            
            return cloudTask;
        }
    }
}
