using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NaviLogger.Service;

namespace NaviLogger.Controllers
{
    [ApiController]
    [Route("/api/v1/logger")]
    public class LoggerController: ControllerBase
    {
        private readonly LoggerService _loggerService;
        
        public LoggerController(LoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("start")]
        public IActionResult StartLogging()
        {
            try
            {
                _loggerService.StartLogging();
            }
            catch (Exception e)
            {
                return Conflict("Seems like logging is already started!");
            }

            return Ok();
        }

        [HttpDelete("stop")]
        public IActionResult StopLogging()
        {
            try
            {
                _loggerService.StopLogging();
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }

            return Ok();
        }
    }
}