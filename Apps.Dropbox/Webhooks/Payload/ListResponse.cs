﻿namespace Apps.Dropbox.Webhooks.Payload;

public class ListResponse<T>
{
   public IEnumerable<T> Files { get; set; }
}