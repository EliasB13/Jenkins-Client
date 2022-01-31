using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jenkins.ConsoleGUI;
using Jenkins.Extensions;
using Jenkins.Models;
using Newtonsoft.Json;

namespace Jenkins
{
    public class Jenkins
    {
        private readonly RequestSender requestSender;

        private readonly string downloadsPath = string.Empty;

        public Jenkins(string userName, string token, string downloadsPath)
        {
            requestSender = new RequestSender(userName, token);

            this.downloadsPath = downloadsPath;
        }

        public async Task<Job?> GetJobByNameAsync(string nameToFind)
        {
            Job? job = null;

            if (!string.IsNullOrEmpty(nameToFind))
            {
                nameToFind = nameToFind.PrepareJobNameToSearch();

                var jobsList = await GetJobsAsync();
                var realJobName = jobsList?.Jobs?.FirstOrDefault(j => j?.Name?.PrepareJobNameToSearch()?.Contains(nameToFind) ?? false)?.Name;

                if (!string.IsNullOrEmpty(realJobName))
                {
                    job = await GetJobByNameExtendedAsync(realJobName);
                }
            }

            return job;
        }

        private async Task<Job?> GetJobByNameExtendedAsync1(string nameToFind)
        {
            Job? job = null;

            if (!string.IsNullOrEmpty(nameToFind))
            {
                var uri = $"{Constants.JenkinsBaseUri}/job/{nameToFind}/api/json?tree=name,builds[number,timestamp,id,result]";

                var response = await requestSender.GetAsync(uri, HttpMethod.Get);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    job = JsonConvert.DeserializeObject<Job>(content);

                    if (job?.Builds?.Count > 0 && !string.IsNullOrEmpty(job?.Name))
                    {
                        job.Builds.ForEach(build =>
                        {
                            build.JobName = job.Name;
                            build.Version = ParseBuildVersion(build);
                        });
                    }
                }
            }

            return job;
        }

        private async Task<Job?> GetJobByNameExtendedAsync(string nameToFind)
        {
            Job? job = null;

            if (!string.IsNullOrEmpty(nameToFind))
            {
                var uri = $"{Constants.JenkinsBaseUri}/job/{nameToFind}/api/json?tree=name,builds[number,timestamp,id,result]";

                var response = await requestSender.GetAsync(uri, HttpMethod.Get);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    job = JsonConvert.DeserializeObject<Job>(content);

                    if (job?.Builds?.Count > 0 && !string.IsNullOrEmpty(job?.Name))
                    {
                        var fetchBuildsTasks = new List<Task<Build?>>();

                        job.Builds.ForEach(build => fetchBuildsTasks.Add(GetBuildAsync(job.Name, build.Number)));

                        var builds = await Task.WhenAll(fetchBuildsTasks);

                        job.Builds = builds.ToList();
                    }
                }
            }

            return job;
        }

        private string ParseBuildVersion(Build build)
        {
            var version = "0.0.0.0";

            if (build != null)
            {
                try
                {
                    var certificateFile = build.Artifacts.FirstOrDefault(a => a.RelativePath.EndsWith(Constants.CertificateExtension))?.FileName;
                    if (!string.IsNullOrEmpty(certificateFile))
                    {
                        var versionRegex = new Regex(@"(?<=_)\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}(?=_)");
                        var versionMatch = versionRegex.Match(certificateFile);
                        if (versionMatch.Success)
                        {
                            version = versionMatch.Value;
                        }
                    }
                }
                catch (Exception) { }
            }

            return version;
        }

        public async Task<Build?> GetBuildAsync(string jobName, int buildNumber)
        {
            Build? build = null;

            if (!string.IsNullOrEmpty(jobName))
            {
                var uri = $"{Constants.JenkinsBaseUri}/job/{jobName}/{buildNumber}/api/json";

                var response = await requestSender.GetAsync(uri, HttpMethod.Get);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    build = JsonConvert.DeserializeObject<Build>(content);
                    if (build != null)
                    {
                        build.JobName = jobName;
                        build.Version = ParseBuildVersion(build);

                        if (build.Result == null && build.Building)
                        {
                            build.Result = Constants.BuildInProgressStatus;
                        }
                    }
                }
            }

            return build;
        }

        public async Task<JobsList?> GetJobsAsync()
        {
            JobsList? jobsList = null;

            using (var client = new HttpClient())
            {
                var uri = $"{Constants.JenkinsJobsUri}[name]";

                var response = await requestSender.GetAsync(uri, HttpMethod.Get);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    jobsList = JsonConvert.DeserializeObject<JobsList>(content);
                }
            }

            return jobsList;
        }

        public void DownloadArtifactsAsync(string uri, string fileName, Action<string>? downloadEndedAction = null)
        {
            _ = Task.Run(async () =>
            {
                using (var client = new HttpClient())
                {
                    var response = await requestSender.GetAsync(uri, HttpMethod.Get);
                    if (response.IsSuccessStatusCode)
                    {
                        using (var fileStream = new FileStream(@$"{downloadsPath}\{fileName}.zip", FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fileStream);

                            downloadEndedAction?.Invoke(fileName);
                        }
                    }
                }
            });
        }
    }
}
