using System;
using System.Collections.Generic;

namespace Ratbags.Articles.API.Models;

public partial class ArticleView
{
    public Guid ViewId { get; set; }

    public Guid ArticleId { get; set; }

    public DateTime? ViewDate { get; set; }

    public string? Ipaddress { get; set; }

    public virtual Article Article { get; set; } = null!;
}
