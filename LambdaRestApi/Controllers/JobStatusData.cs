using Newtonsoft.Json.Linq;

namespace LambdaRestApi.Controllers
{
    public class JobStatusData
    {
        public JobStatus Status { get; set; }
        public JObject Data { get; set; }
        public int? QueueIndex { get; set; }

        public JobStatusData(JobData data, JobStatus status)
        {
            Status = status;
            Data = data.ToJson();

            if (data.RunException != null)
                Status = JobStatus.Failed;
        }

        public JobStatusData(JobData data, JobStatus status, int queueIndex) : this(data, status)
        {
            QueueIndex = queueIndex;
        }
    }
}