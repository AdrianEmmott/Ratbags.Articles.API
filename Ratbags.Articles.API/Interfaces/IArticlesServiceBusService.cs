namespace Ratbags.Articles.API.Interfaces
{
    public interface IArticlesServiceBusService
    {
        Task<Dictionary<Guid, int>?> GetArticlesCommentsCount(List<Guid> ids);
    }
}
