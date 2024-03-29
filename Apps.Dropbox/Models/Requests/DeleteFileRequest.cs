﻿using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests;

public class DeleteFileRequest
{
    [Display("File path")]
    [DataSource(typeof(FileDataSourceHandler))]
    public string FilePath { get; set; }
}