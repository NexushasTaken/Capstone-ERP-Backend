namespace ERP.Repository.Interface.CurrentUser
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Role { get; }
    }
}
