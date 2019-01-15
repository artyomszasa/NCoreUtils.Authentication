namespace NCoreUtils.Authentication
{
    public interface IUsernameFormatter
    {
        string Format(IUser user);
    }
}