using System;
using System.Collections.Generic;

namespace Api;

public partial class Image
{
    public long Id { get; set; }

    public string? Path { get; set; }

    public string? PreviewPath { get; set; }
}
