using Emart_DotNet.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emart_DotNet.Repositories
{


    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Address> SaveAsync(Address address)
        {
            if (address.AddressId == 0)
            {
                _context.Addresses.Add(address);
            }
            else
            {
                 _context.Addresses.Update(address);
            }
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<List<Address>> FindByUserIdAsync(int userId)
        {
            return await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task DeleteAsync(int addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Address?> FindByIdAsync(int addressId)
        {
            return await _context.Addresses.FindAsync(addressId);
        }
    }
}
