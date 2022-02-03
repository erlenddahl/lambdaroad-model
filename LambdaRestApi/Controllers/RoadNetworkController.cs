using System;
using System.Collections.Generic;
using System.Linq;
using LambdaModel.Config;
using LambdaRestApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
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
        private static readonly Dictionary<string, JobData> FinishedJobs = new();
        private static JobData _currentJob;
        private readonly string _resultsDirectory;

        public RoadNetworkController(ILogger<RoadNetworkController> logger)
        {
            _logger = logger;
            _resultsDirectory = @"C:\Code\LambdaModel\Data\ApiOutput";
        }

        [HttpPost]
        public object Start(RoadNetworkConfig config)
        {
            config.CalculationMethod = CalculationMethod.RoadNetwork;
            config.RoadShapeLocation = @"C:\Code\LambdaModel\Data\RoadNetwork\2021-05-28_smaller.shp";

            foreach(var bs in config.BaseStations)
                bs.Initialize();

            config.Terrain = new TerrainConfig()
            {
                Type = TerrainType.LocalCache,
                Location = @"I:\Jobb\Lambda\Tiles_512",
                MaxCacheItems = 300,
                RemoveCacheItemsWhenFull = 100,
                TileSize = 512
            };

            var job = new JobData(config, _resultsDirectory);

            JobQueue.Enqueue(job);
            ProcessQueue();
            
            return JobStatus(job.Id);
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
                    WmsUrl = "https://wms.geonorge.no/skwms1/wms.hoyde-dom?bbox={0}&format=image/tiff&service=WMS&version=1.1.1&request=GetMap&srs=EPSG:25833&transparent=true&width={1}&height={2}&layers=dom1_33:None",
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
            {
                FinishedJobs.Add(_currentJob.Id, _currentJob);
                _currentJob = null;
            }

            foreach(var job in FinishedJobs)
                if (!job.Value.HasBeenSaved)
                    job.Value.Save();

            if (_currentJob != null) return;
            if (!JobQueue.Any()) return;

            _currentJob = JobQueue.Dequeue();
            _currentJob.Run(ProcessQueue);
        }

        [HttpGet]
        public object JobStatus(string key)
        {
            if (_currentJob?.Id == key) return new JobStatusData(_currentJob, Controllers.JobStatus.Processing);

            if (FinishedJobs.TryGetValue(key, out var job))
                return new JobStatusData(job, Controllers.JobStatus.Finished);

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

        [HttpGet("results")]
        public object Results(string key)
        {
            var dir = System.IO.Path.Combine(_resultsDirectory, key);
            if (!System.IO.Directory.Exists(dir)) throw new NoSuchResultsException();

            var metaFile = System.IO.Path.Combine(dir, "meta.json");
            if (!System.IO.File.Exists(metaFile)) throw new ResultsMissingMetadataException();
            var meta = JsonConvert.DeserializeObject<RoadLinkResultMetadata[]>(System.IO.File.ReadAllText(metaFile));

            return meta;
        }

        [HttpGet("jobs")]
        public object Jobs(string key)
        {
            if (!System.IO.Directory.Exists(_resultsDirectory)) return new string[0];
            return System.IO.Directory.GetDirectories(_resultsDirectory)
                .Select(p =>
                {
                    var jobData = System.IO.Path.Combine(p, "jobdata.json");
                    if (!System.IO.File.Exists(jobData)) return null;
                    try
                    {
                        return JobData.FromFile(jobData);
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                })
                .Where(p => p != null)
                .ToArray();
        }
    }
}
