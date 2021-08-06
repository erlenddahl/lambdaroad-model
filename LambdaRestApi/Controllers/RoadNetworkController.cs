using System;
using System.Collections.Generic;
using System.Linq;
using LambdaModel.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LambdaRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoadNetworkController : ControllerBase
    {
        private readonly ILogger<RoadNetworkController> _logger;
        
        private static Queue<JobData> _jobQueue = new Queue<JobData>();
        private static Dictionary<string, JobData> _finishedJobs = new Dictionary<string, JobData>();
        private static JobData _currentJob = null;

        public RoadNetworkController(ILogger<RoadNetworkController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public JobStatusData Start(RoadNetworkConfig config)
        {
            config.CalculationMethod = CalculationMethod.RoadNetwork;
            config.TileSize = 512;
            config.RoadShapeLocation = @"C:\Code\LambdaModel\Data\RoadNetwork\2021-05-28_smaller.shp";
            config.OutputLocation = @"C:\Code\LambdaModel\Data\RoadNetwork\test-results-huge.shp";

            config.Terrain = new TerrainConfig()
            {
                Type = TerrainType.LocalCache,
                Location = @"I:\Jobb\Lambda\Tiles_512",
                MaxCacheItems = 300,
                RemoveCacheItemsWhenFull = 100
            };

            config.Validate();

            var job = new JobData(config);
            _jobQueue.Enqueue(job);
            ProcessQueue();
            
            return JobStatus(job.Id);
        }

        private void ProcessQueue()
        {
            if (_currentJob?.Config.FinalSnapshot != null)
            {
                _finishedJobs.Add(_currentJob.Id, _currentJob);
                _currentJob = null;
            }
            if (_currentJob != null) return;
            if (!_jobQueue.Any()) return;

            _currentJob = _jobQueue.Dequeue();
            _currentJob.Run(ProcessQueue);
        }

        [HttpGet]
        public JobStatusData JobStatus(string key)
        {
            if (_currentJob?.Id == key) return new JobStatusData(_currentJob, Controllers.JobStatus.Processing);

            if (_finishedJobs.TryGetValue(key, out var job))
                return new JobStatusData(job, Controllers.JobStatus.Finished);

            var ix = 0;
            foreach (var q in _jobQueue)
            {
                if (q.Id == key)
                {
                    return new JobStatusData(q, Controllers.JobStatus.InQueue, ix);
                }
                ix++;
            }

            return null;
        }
    }
}
