using System;
using LambdaModel.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LambdaRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SinglePointController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SinglePointController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public object Start(SinglePointConfig config)
        {
            try
            {
                config.Terrain = new TerrainConfig()
                {
                    Type = TerrainType.LocalCache,
                    Location = _config.GetValue<string>("TileCacheLocation"),
                    MaxCacheItems = 300,
                    RemoveCacheItemsWhenFull = 100,
                    TileSize = 512
                };

                return config.Run();
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
}