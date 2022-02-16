using System;
using System.Diagnostics;
using System.Threading.Tasks;
using LambdaModel.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LambdaRestApi.Controllers
{
    public class JobData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public RoadNetworkConfig Config { get; }

        public int CreatedBy { get; set; }

        public DateTime Enqueued { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }

        [JsonIgnore]
        public Task RunTask { get; set; }
        public Exception RunException { get; set; }

        public bool HasBeenSaved { get; set; } = false;

        public JobData()
        {

        }

        public JobData(RoadNetworkConfig config, string resultsDirectory)
        {
            Config = config;
            Enqueued = DateTime.Now;
            CreatedBy = -1;
            Started = DateTime.MinValue;
            Finished = DateTime.MinValue;

            Config.OutputDirectory = System.IO.Path.Combine(resultsDirectory, Id);
            config.Validate();
        }

        public void Run(Action callback)
        {
            Started = DateTime.Now;
            RunTask = Task.Run(() =>
            {
                try
                {
                    Config.Run();
                }
                catch (Exception ex)
                {
                    RunException = ex;
                    Debug.WriteLine(ex.StackTrace);
                }
                Finished = DateTime.Now;

                Save();

                callback();
            });
        }

        public void Save()
        {
            if (HasBeenSaved) return;

            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(Config.OutputDirectory, "jobstatusdata.json"), JsonConvert.SerializeObject(new JobStatusData(this, JobStatus.Finished)));

                System.IO.File.WriteAllText(System.IO.Path.Combine(Config.OutputDirectory, "jobdata.json"), ToJson().ToString());
                
                var c = JObject.FromObject(Config);
                c.Remove(nameof(Config.Cip));
                c.Remove(nameof(Config.FinalSnapshot));
                
                System.IO.File.WriteAllText(System.IO.Path.Combine(Config.OutputDirectory, "config.json"), c.ToString());
                HasBeenSaved = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public JObject ToJson()
        {
            var json = new JObject()
            {
                {nameof(Id), Id},
                {nameof(CreatedBy), CreatedBy},
                {nameof(Enqueued), Enqueued}
            };

            if (Started > DateTime.MinValue)
                json[nameof(Started)] = Started;
            if (Started > DateTime.MinValue)
                json[nameof(Finished)] = Finished;
            if (RunException != null)
                json[nameof(RunException)] = RunException.Message;

            var snap = Config?.FinalSnapshot ?? Config?.Cip?.GetSnapshot();
            if (snap != null)
                json.Add("Snapshot", JObject.FromObject(snap));

            return json;
        }

        public static JobData FromFile(string path)
        {
            return JsonConvert.DeserializeObject<JobData>(System.IO.File.ReadAllText(path));
        }
    }
}