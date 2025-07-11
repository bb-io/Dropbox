﻿using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Webhooks.Inputs;

public class ParentFolderInput
{
    [Display("Folder path")]
    [DataSource(typeof(FolderDataSourceHandler))]
    public string? ParentFolderLowerPath { get; set; }
}