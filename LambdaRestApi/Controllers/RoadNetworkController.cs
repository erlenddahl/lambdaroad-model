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
            config.TileSize = 512;
            config.RoadShapeLocation = @"C:\Code\LambdaModel\Data\RoadNetwork\2021-05-28_smaller.shp";

            config.Terrain = new TerrainConfig()
            {
                Type = TerrainType.LocalCache,
                Location = @"I:\Jobb\Lambda\Tiles_512",
                MaxCacheItems = 300,
                RemoveCacheItemsWhenFull = 100
            };

            var job = new JobData(config, _resultsDirectory);

            JobQueue.Enqueue(job);
            ProcessQueue();
            
            return JobStatus(job.Id);
        }

        private void ProcessQueue()
        {
            if (_currentJob != null && _currentJob.Finished > DateTime.MinValue)
            {
                FinishedJobs.Add(_currentJob.Id, _currentJob);
                _currentJob = null;
            }
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
    }
}
