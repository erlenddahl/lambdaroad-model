using LambdaModel.Config;
using Microsoft.AspNetCore.Mvc;

namespace LambdaRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SinglePointController : ControllerBase
    {
        [HttpPost]
        public object Start(SinglePointConfig config)
        {
            config.Terrain = new TerrainConfig()
            {
                Type = TerrainType.LocalCache,
                Location = @"I:\Jobb\Lambda\Tiles_512",
                MaxCacheItems = 300,
                RemoveCacheItemsWhenFull = 100
            };

            return config.Run();
        }
    }
}