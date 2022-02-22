﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly string _resultsDirectory;
        private readonly IConfiguration _config;

        public RoadNetworkController(ILogger<RoadNetworkController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _resultsDirectory = _config.GetValue<string>("ResultsLocation");
        }

        [HttpPost]
        public object Start(RoadNetworkConfig config)
        {
            string jobId;
            try
            {
                config.CalculationMethod = CalculationMethod.RoadNetwork;
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

                var job = new JobData(config, _resultsDirectory);

                JobQueue.Enqueue(job);

                jobId = job.Id;
            }
            catch (Exception ex)
            {
                return new
                {
                    error = ex.Message
                };
            }

            ProcessQueue();

            return JobStatus(jobId);
        }

        [HttpPost("generateConfig")]
        public object GenerateConfig(RoadNetworkConfig config)
        {
            try
            {
                config.OutputDirectory = "[INSERT ACTUAL PATH HERE]";
                config.CalculationMethod = CalculationMethod.RoadNetwork;
                config.RoadShapeLocation = "[INSERT ACTUAL PATH HERE]";
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

        private void ProcessQueue()
        {
            if (_currentJob != null && _currentJob.Finished > DateTime.MinValue)
                _currentJob = null;

            if (_currentJob != null) return;
            if (!JobQueue.Any()) return;

            _currentJob = JobQueue.Dequeue();
            _currentJob.Run(ProcessQueue);
        }

        [HttpGet]
        public object JobStatus(string key)
        {
            try
            {
                if (_currentJob?.Id == key) return new JobStatusData(_currentJob, Controllers.JobStatus.Processing);

                var ix = 0;
                foreach (var q in JobQueue)
                {
                    if (q.Id == key)
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

        [HttpGet("results")]
        public object Results(string key)
        {
            var dir = Path.Combine(_resultsDirectory, key);
            if (!Directory.Exists(dir)) throw new NoSuchResultsException();

            var metaFile = Path.Combine(dir, "links", "meta.json");
            if (!System.IO.File.Exists(metaFile)) throw new ResultsMissingMetadataException();
            return JObject.Parse(System.IO.File.ReadAllText(metaFile));
        }

        [HttpGet("jobs")]
        public object Jobs()
        {
            if (!Directory.Exists(_resultsDirectory)) return new string[0];
            return Directory.GetDirectories(_resultsDirectory)
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
                .Concat(new[] {_currentJob == null ? null : new JobStatusData(_currentJob, Controllers.JobStatus.Processing)})
                .Concat(JobQueue.Select(p => new JobStatusData(p, Controllers.JobStatus.InQueue)))
                .Where(p => p != null)
                .OrderBy(p => p.Data[nameof(JobData.Enqueued)])
                .ToArray();
        }
    }
}
