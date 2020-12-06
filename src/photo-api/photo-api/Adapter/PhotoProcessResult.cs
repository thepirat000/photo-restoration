using System.Collections.Generic;

namespace photo_api.Adapter
{
    public class PhotoProcessResult
    {
        public int ExitCode { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string OutputFolder { get; set; }
        public string Output { get; set; }

        public string TraceId { get; set; }
    }

}
