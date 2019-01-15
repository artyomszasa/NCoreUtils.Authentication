namespace NCoreUtils.Authentication
{
    public interface ILocalUser<TId> : IUser<TId>
    {
        string Password { get; }

        string Salt { get; }
    }
}