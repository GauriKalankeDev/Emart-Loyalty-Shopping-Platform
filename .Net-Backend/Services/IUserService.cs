using Emart_DotNet.Models;
using System.Threading.Tasks;

namespace Emart_DotNet.Services
{
    public interface IUserService
    {
        Task<Customer> LoginAsync(string email, string password);
        Task<Customer> RegisterUserAsync(Customer customer);
        Task<Customer> ProcessGoogleLoginAsync(string email, string fullName);
        Task<Customer> CompleteRegistrationAsync(int userId, Customer customer);
        Task<Customer> GetUserByIdAsync(int userId);
        Task<Customer> UpdateUserAsync(int userId, Customer customer);
    }
}
