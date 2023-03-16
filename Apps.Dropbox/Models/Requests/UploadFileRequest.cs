﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Dropbox.Models.Requests
{
    public class UploadFileRequest
    {
        public string Path { get; set; }

        public string Filename { get; set; }

        public string FileType { get; set; }

        public byte[] File { get; set; }
    }
}
