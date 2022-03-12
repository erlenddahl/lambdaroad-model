using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Extensions.StringExtensions;
using Extensions.Utilities.Csv;
using LambdaModel.Config;
using LambdaModel.General;
using LambdaModel.Stations;
using LambdaRestApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LambdaRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoadNetworkController : ControllerBase
    {
        private readonly ILogger<RoadNetworkController> _logger;
        
        private static readonly Queue<JobData> JobQueue = new();
        private static JobData _currentJob;
        private static readonly object LockObject = new();

        private readonly string _resultsBaseDirectory;
        private readonly IConfiguration _config;

        private static Dictionary<string, string> _apiKeys = new();

        public RoadNetworkController(ILogger<RoadNetworkController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _resultsBaseDirectory = _config.GetValue<string>("ResultsLocation");
            LoadApiKeys();
        }

        private void LoadApiKeys()
        {
            try
            {
                var keyLocation = _config.GetValue<string>("KeyLocation");
                if (!System.IO.File.Exists(keyLocation)) return;

                var csvReader = new CsvReader();
                _apiKeys = csvReader.ReadFile(keyLocation).ToDictionary(k => k["key"], v => v["owner"]);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Failed to load API keys");
            }
        }

        [HttpPost]
        public object Start(RoadNetworkConfig config)
        {
            try
            {
                ValidateApiKey(config.ApiKey);

                config.Operation = OperationType.RoadNetwork;
                config.RoadShapeLocation = _config.GetValue<string>("RoadShapeLocation");

                foreach (var bs in config.BaseStations)
                    bs.Initialize();

                config.Terrain = new TerrainConfig()
                {
                    Type = TerrainType.LocalCache,
                    Location = _config.GetValue<string>("TileCacheLocation"),
                    MaxCacheItems = 300,
                    RemoveCacheItemsWhenFull = 100,
                    TileSize = 512
                };

                var job = new JobData(config, GetResultsDirectory(config.ApiKey));

                lock (LockObject)
                {
                    JobQueue.Enqueue(job);
                }

                return JobStatus(new BasicParameters() {ApiKey = config.ApiKey, Key = job.Id});
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
            finally
            {
                ProcessQueue();
            }
        }

        private string GetResultsDirectory(string key)
        {
            return Path.Combine(_resultsBaseDirectory, key.MakeSafeForFilename());
        }

        private void ValidateApiKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("API key must be provided");

            if (!_apiKeys.ContainsKey(key))
                LoadApiKeys();

            if (!_apiKeys.ContainsKey(key))
                throw new Exception("The provided API key was not valid.");
        }

        [HttpPost("generateConfig")]
        public object GenerateConfig(RoadNetworkConfig config)
        {
            try
            {
                config.OutputDirectory = "[INSERT ACTUAL PATH HERE]";
                config.Operation = OperationType.RoadNetwork;
                config.RoadShapeLocation = "[INSERT ACTUAL PATH HERE]";
                config.WriteApiResults = false;
                config.Terrain = new TerrainConfig()
                {
                    Type = TerrainType.OnlineCache,
                    Location = "[INSERT ACTUAL PATH HERE]",
                    WmsUrl = _config.GetValue<string>("TileWmsUrl"),
                    MaxCacheItems = 300,
                    RemoveCacheItemsWhenFull = 100,
                    TileSize = 512
                };

                var json = JObject.FromObject(config);
                foreach (JObject bs in json[nameof(config.BaseStations)])
                    bs.Value<JObject>("Center").Remove("Z");
                return json;
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }

        [HttpGet("download")]
        public object Download(BasicParameters parameters)
        {
            try
            {
                ValidateApiKey(parameters.ApiKey);

                var dir = Path.Combine(GetResultsDirectory(parameters.ApiKey), parameters.Key);
                if (!Directory.Exists(dir)) throw new NoSuchResultsException();

                if (parameters.Format == "csv")
                {
                    var file = Path.Combine(dir, "results.csv");
                    if (!System.IO.File.Exists(file)) throw new ResultsMissingMetadataException();
                    return File(System.IO.File.Open(file, FileMode.Open), "text/csv", "lambda-export.csv");
                }

                if (parameters.Format == "shp")
                {
                    var file = Path.Combine(dir, "results.zip");
                    if (!System.IO.File.Exists(file)) throw new ResultsMissingMetadataException();
                    return File(System.IO.File.Open(file, FileMode.Open), "application/zip", "lambda-export.zip");
                }

                throw new Exception("Invalid format specified. Must be csv or shp.");
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }

        private void ProcessQueue()
        {
            lock (LockObject)
            {
                if (_currentJob != null && _currentJob.Finished > DateTime.MinValue)
                    _currentJob = null;

                if (_currentJob != null) return;
                if (!JobQueue.Any()) return;

                _currentJob = JobQueue.Dequeue();
            }

            _currentJob.Run(ProcessQueue);
        }

        [HttpPost("status")]
        public object JobStatus(BasicParameters parameters)
        {
            try
            {
                ValidateApiKey(parameters.ApiKey);

                if (_currentJob?.Id == parameters.Key) return new JobStatusData(_currentJob, Controllers.JobStatus.Processing);

                var ix = 0;
                foreach (var q in JobQueue)
                {
                    if (q.Id == parameters.Key)
                    {
                        return new JobStatusData(q, Controllers.JobStatus.InQueue, ix);
                    }

                    ix++;
                }

                return new JObject();
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }

        [HttpPost("results")]
        public object Results(BasicParameters parameters)
        {
            try
            {
                ValidateApiKey(parameters.ApiKey);

                var dir = Path.Combine(GetResultsDirectory(parameters.ApiKey), parameters.Key);
                if (!Directory.Exists(dir)) throw new NoSuchResultsException();

                var metaFile = Path.Combine(dir, "links", "meta.json");
                if (!System.IO.File.Exists(metaFile)) throw new ResultsMissingMetadataException();
                return JObject.Parse(System.IO.File.ReadAllText(metaFile));
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }

        [HttpPost("delete")]
        public object DeleteJob(BasicParameters parameters)
        {
            try
            {
                ValidateApiKey(parameters.ApiKey);

                var dir = Path.Combine(GetResultsDirectory(parameters.ApiKey), parameters.Key);
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);

                lock (LockObject)
                {
                    if (JobQueue.Any(p => p.Id == parameters.Key))
                    {
                        var jobs = JobQueue.Where(p => p.Id != parameters.Key).ToArray();
                        JobQueue.Clear();
                        foreach (var job in jobs)
                            JobQueue.Enqueue(job);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }

        [HttpPost("abort")]
        public object AbortRunningJob(BasicParameters parameters)
        {
            try
            {
                ValidateApiKey(parameters.ApiKey);

                if (_currentJob == null) throw new Exception("No currently running job to abort.");
                if(_currentJob.Id != parameters.Key) throw new Exception("Currently running job ID did not match.");
                _currentJob.Config.Cancellor.Cancel();
                return true;
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }

        [HttpPost("jobs")]
        public object Jobs(BasicParameters parameters)
        {
            try
            {
                ValidateApiKey(parameters.ApiKey);

                var resultsDirectory = GetResultsDirectory(parameters.ApiKey);
                if (!Directory.Exists(resultsDirectory)) return new string[0];
                return Directory.GetDirectories(resultsDirectory)
                    .Select(p =>
                    {
                        var jobStatusData = Path.Combine(p, "jobstatusdata.json");
                        if (!System.IO.File.Exists(jobStatusData)) return null;
                        try
                        {
                            return JobStatusData.FromFile(jobStatusData);
                        }
                        catch (Exception ex)
                        {
                            JobData job;
                            try
                            {
                                job = JobData.FromFile(Path.Combine(p, "jobdata.json"));
                                job.RunException = ex;
                            }
                            catch (Exception ex2)
                            {
                                job = new JobData()
                                {
                                    Id = Path.GetFileName(Path.GetDirectoryName(p)),
                                    RunException = ex2
                                };
                            }

                            return new JobStatusData(job, Controllers.JobStatus.Failed);
                        }
                    })
                    .Concat(new[] {(_currentJob == null || _currentJob.Config.ApiKey != parameters.ApiKey) ? null : new JobStatusData(_currentJob, Controllers.JobStatus.Processing)})
                    .Concat(JobQueue.Where(p => p.Config.ApiKey == parameters.ApiKey).Select(p => new JobStatusData(p, Controllers.JobStatus.InQueue)))
                    .Where(p => p != null)
                    .OrderBy(p => p.Data[nameof(JobData.Enqueued)])
                    .ToArray();
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }
        }
    }

    public class BasicParameters
    {
        public string ApiKey { get; set; }
        public string Key { get; set; }
        public string Format { get; set; }
    }
}
