using Emart_DotNet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emart_DotNet.Services
{
    public interface IAddressService
    {
        Task<Address> AddAddressAsync(int userId, Address address);
        Task<List<Address>> GetAddressesByCustomerAsync(int userId);
        Task DeleteAddressAsync(int addressId);
    }
}
