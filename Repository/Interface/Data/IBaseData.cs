namespace ERP.Repository.Interface.Data
{
    public interface IBaseData
    {
        Task Save<T>(T data, CancellationToken cancellation = default)
            where T : class;
        Task SaveMany<T>(IEnumerable<T> data, CancellationToken cancellation = default)
            where T : class;
        Task SaveChanges(CancellationToken cancellation = default);
        IQueryable<T> BaseQuery<T>(bool withTracking)
            where T : class;
    }
}
