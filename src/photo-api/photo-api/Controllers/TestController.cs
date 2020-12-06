using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Audit.WebApi;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace photo_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors]
    [AuditApi]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            var moduleFile = Process.GetCurrentProcess().MainModule.FileName;
            var lastModified = System.IO.File.GetLastWriteTime(moduleFile);
            var x = JsonSerializer.Serialize(new
            {
                Environment.MachineName,
                OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                OSDescription = RuntimeInformation.OSDescription.ToString(),
                BuildDate = lastModified,
                Environment.ProcessorCount
            });
            return x;

        }
    }
}
