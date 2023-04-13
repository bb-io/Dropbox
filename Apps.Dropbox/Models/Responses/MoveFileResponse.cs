using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Dropbox.Models.Responses
{
    public class MoveFileResponse
    {
        public string FileName { get; set; }

        public string NewFilePath { get; set; }
    }
}
