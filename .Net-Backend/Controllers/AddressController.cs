using Emart_DotNet.Models;
using Emart_DotNet.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emart_DotNet.Controllers
{
    [ApiController]
    [Route("api/address")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Address>>> GetUserAddresses(int userId)
        {
            return Ok(await _addressService.GetAddressesByCustomerAsync(userId));
        }

        [HttpPost("add/{userId}")]
        public async Task<ActionResult<Address>> AddAddress(int userId, [FromBody] Address address)
        {
            return Ok(await _addressService.AddAddressAsync(userId, address));
        }

        [HttpDelete("{addressId}")]
        public async Task<ActionResult> DeleteAddress(int addressId)
        {
            await _addressService.DeleteAddressAsync(addressId);
            return Ok("Address deleted successfully");
        }
    }
}
