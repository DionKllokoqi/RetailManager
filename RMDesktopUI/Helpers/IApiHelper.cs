using RMDesktopUI.Models;
using System.Threading.Tasks;

namespace RMDesktopUI.Helpers
{
    public interface IApiHelper
    {
        Task<AuthenticatedUser> Authenticate(string username, string password);
    }
}