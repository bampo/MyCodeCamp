using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private readonly ILogger<OperationsController> _logger;
        private readonly IConfigurationRoot _config;

        public OperationsController(ILogger<OperationsController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config as IConfigurationRoot;
        }
        [HttpOptions("reloadConfig")]
        public IActionResult ReloadConfiguration()
        {
            try
            {
                _config.Reload();
                
                return Ok("Configuration reloaded");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while reloading configuration: {e}");
            }

            return BadRequest("Couldn't reload configuration");
        }
    }
}