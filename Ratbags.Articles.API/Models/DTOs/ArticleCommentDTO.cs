namespace Ratbags.Articles.API.Models.DTOs;

public sealed record ArticleCommentDTO(
    Guid Id,
    string? Username,
    string Content,
    DateTimeOffset Published
);
