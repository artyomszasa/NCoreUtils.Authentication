using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public interface IUser
    {
        string Email { get; }

        string FamilyName { get; }

        string GivenName { get; }
    }

    public interface IUser<TId> : IUser
    {
        TId Id { get; }
    }
}