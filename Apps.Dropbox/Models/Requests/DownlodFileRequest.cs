using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Dropbox.Models.Requests
{
    public class DownlodFileRequest
    {
        public string Path { get; set; }

        public string FileName { get; set; }
    }
}
