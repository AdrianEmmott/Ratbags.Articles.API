using System;
using System.Collections.Generic;

namespace Ratbags.Articles.API.Models;

public partial class ArticleLike
{
    public Guid LikeId { get; set; }

    public Guid ArticleId { get; set; }

    public string? LikedBy { get; set; }

    public DateTime? LikeDate { get; set; }

    public virtual Article Article { get; set; } = null!;
}
