using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Dropbox.Models.Responses
{
    public class GetDownloadLinkResponse
    {
        public string LinkForDownload { get; set; }

        public string Path { get; set; }

        public ulong Size { get; set; }
    }
}
