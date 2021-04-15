using RMDesktopUI.Library.Models;
using System.Threading.Tasks;

namespace RMDesktopUI.Library.Api
{
    public interface IApiHelper
    {
        Task<AuthenticatedUser> Authenticate(string username, string password);
        Task GetLoggedInUserInfo(string token);
    }
}