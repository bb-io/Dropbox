using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Dropbox.Models.Requests
{
    public class CreateFolderRequest
    {
        public string Path { get; set; }

        public string FolderName { get; set; }
    }
}
