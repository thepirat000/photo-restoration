using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using photo_api.Adapter;
using Audit.WebApi;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using photo_api.Shell;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace photo_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [RequestFormLimits(ValueCountLimit = 5000)]
    [EnableCors]
    [AuditApi(IncludeHeaders = true, IncludeResponseBody = true, IncludeResponseHeaders = true)]
    public class PhotoRestorationController : ControllerBase
    {
        private static long Max_Upload_Size = long.Parse(Startup.Configuration["AppSettings:MaxUploadSize"]);
        private static string InputFolderRoot = Startup.Configuration["AppSettings:InputFolder"];
        private static string ZipFolderRoot = Startup.Configuration["AppSettings:ZipFolder"];

        private readonly ILogger<PhotoRestorationController> _logger;
        private readonly PhotoAdapter _photoAdapter = new PhotoAdapter();

        public PhotoRestorationController(ILogger<PhotoRestorationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("p")]
        [Produces("application/json")]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        public async Task<ActionResult<PhotoProcessResult>> Process([FromQuery(Name = "gpu")] string gpu)
        {
            if (Request.Form.Files?.Count == 0)
            {
                return BadRequest("No images to process");
            }

            var totalBytes = Request.Form.Files.Sum(f => f.Length);
            if (totalBytes > Max_Upload_Size)
            {
                return BadRequest($"Can't process more than {Max_Upload_Size / 1024:N0} Mb of data");
            }

            var traceId = this.HttpContext.TraceIdentifier.Replace(":", "");
            Startup.EphemeralLog($"Starting process for trace {traceId}");
            // TODO: Check if file exists

            var result = await ProcessImpl(traceId, gpu);
            return Ok(result);
        }

        [HttpGet("d")]
        public ActionResult Download([FromQuery(Name = "t")] string traceId)
        {
            if (string.IsNullOrWhiteSpace(traceId))
            {
                return BadRequest();
            }
            if (traceId != ShellHelper.SanitizeFilename(traceId))
            {
                return BadRequest("Don't try to cheat me");
            }
            var zipFile = Path.Combine(ZipFolderRoot, $"{traceId}-restored.zip");
            if (System.IO.File.Exists(zipFile))
            {
                return PhysicalFile(zipFile, "application/zip", $"{traceId}-restored.zip");
            }
            return Problem($"File {zipFile} not found");
        }

        private async Task<PhotoProcessResult> ProcessImpl(string traceId, string gpu)
        {
            // Copy images to input folder
            var inputImagesFolder = Path.Combine(InputFolderRoot, traceId);
            Directory.CreateDirectory(inputImagesFolder);
            foreach (var file in Request.Form.Files)
            {
                var fileName = ShellHelper.SanitizeFilename(file.FileName);
                if (string.IsNullOrEmpty(Path.GetExtension(file.FileName)))
                {
                    // Assume is a jpg
                    fileName += ".jpg";
                }
                var filePath = $"{inputImagesFolder}/{fileName}";
                if (!System.IO.File.Exists(filePath))
                {
                    using (var output = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(output);
                    }
                }
            }

            // Execute
            var result = _photoAdapter.Execute(traceId, gpu);
            if (result.ErrorCount > 0)
            {
                throw new Exception(string.Join(" +++ ", result.Errors));
            }

            // Make zip
            var outputZip = Path.Combine(ZipFolderRoot, $"{traceId}-restored.zip");
            MakeZip(result.OutputFolder, outputZip);

            // Delete temp folders
            //Directory.Delete(inputImagesFolder, true);

            result.TraceId = traceId;

            return result;
        }

        private void MakeZip(string inputFolder, string outputFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
            using (var zip = new FileStream(outputFilePath, FileMode.Create))
            {
                using (var archive = new ZipArchive(zip, ZipArchiveMode.Create))
                {
                    foreach(var file in Directory.GetFiles(inputFolder))
                    {
                        archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }
            }
        }

        private string GetFileHash(IFormFile file)
        {
            SHA256Managed sha = new SHA256Managed();
            using (var stream = new MemoryStream())
            {
                file.OpenReadStream().CopyTo(stream);
                byte[] hash = sha.ComputeHash(stream.ToArray());
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }



    }
}
