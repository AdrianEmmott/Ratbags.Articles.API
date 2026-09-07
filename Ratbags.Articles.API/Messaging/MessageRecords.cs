namespace Ratbags.Articles.API.Messaging;

// article comment counts
public sealed record GetCommentCountsForArticlesRequest(IReadOnlyList<Guid> ArticleIds);
public sealed record GetCommentCountsForArticlesResponse(Dictionary<Guid, int> Counts);

// get usernames
public sealed record GetUserNameDetailsRequest(IReadOnlyList<Guid> userIds);
public sealed record GetUserNameDetailsResponse(Dictionary<Guid, string>? userNameDetails);
