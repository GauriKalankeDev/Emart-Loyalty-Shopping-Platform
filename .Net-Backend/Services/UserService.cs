using Emart_DotNet.Models;
using Emart_DotNet.Repositories;
using Emart_DotNet.Utilities.Helpers;
using System;
using System.Threading.Tasks;

namespace Emart_DotNet.Services
{
    public class UserService : IUserService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly PasswordHelper _passwordHelper;

        public UserService(ICustomerRepository customerRepository, PasswordHelper passwordHelper)
        {
            _customerRepository = customerRepository;
            _passwordHelper = passwordHelper;
        }

        public async Task<Customer> LoginAsync(string email, string password)
        {
            var user = await _customerRepository.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            
            // Allow plain text for migration/testing if hash check fails (or just strict check)
             if (!_passwordHelper.VerifyPassword(password, user.Password))
            {
                 if (user.Password != password) 
                 {
                     throw new Exception("Invalid credentials");
                 }
            }
            return user;
        }

        public async Task<Customer> RegisterUserAsync(Customer customer)
        {
            if (await _customerRepository.FindByEmailAsync(customer.Email) != null)
            {
                throw new Exception("Email already in use");
            }
            if (!string.IsNullOrEmpty(customer.Password))
            {
                customer.Password = _passwordHelper.HashPassword(customer.Password);
            }
            customer.Role = "ROLE_USER"; // Default role
            customer.AuthProvider = "LOCAL";
            customer.ProfileCompleted = 1;
            await _customerRepository.SaveAsync(customer);
            return customer;
        }

        public async Task<Customer> ProcessGoogleLoginAsync(string email, string fullName)
        {
            var user = await _customerRepository.FindByEmailAsync(email);
            if (user != null)
            {
                // Check if existing user is LOCAL
                if (user.AuthProvider == "LOCAL")
                {
                    throw new Exception("This email is registered with a password. Please login using email/password.");
                }
                return user;
            }

            // Create new Google User
            user = new Customer
            {
                Email = email,
                FullName = fullName,
                Role = "ROLE_USER",
                Password = null,
                AuthProvider = "GOOGLE",
                Mobile = "",
                Epoints = 0,
                ProfileCompleted = 0 // False
            };
            
            await _customerRepository.SaveAsync(user);
            return user;
        }

        public async Task<Customer> CompleteRegistrationAsync(int userId, Customer customerUpdates)
        {
            var user = await _customerRepository.FindByUserIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (user.AuthProvider != "GOOGLE")
            {
                throw new Exception("Registration completion is only for Google users");
            }

            // Update fields
            if (!string.IsNullOrEmpty(customerUpdates.FullName)) user.FullName = customerUpdates.FullName;
            if (!string.IsNullOrEmpty(customerUpdates.Mobile)) user.Mobile = customerUpdates.Mobile;
            if (customerUpdates.BirthDate.HasValue) user.BirthDate = customerUpdates.BirthDate;
            if (!string.IsNullOrEmpty(customerUpdates.Interests)) user.Interests = customerUpdates.Interests;
            if (customerUpdates.PromotionalEmail.HasValue) user.PromotionalEmail = customerUpdates.PromotionalEmail;

            // Address logic - If a new address is provided or ID linked
            if (customerUpdates.AddressId != null && customerUpdates.AddressId != 0)
            {
                user.AddressId = customerUpdates.AddressId;
            }
            
            // Mark profile as completed
            user.ProfileCompleted = 1; // True

            await _customerRepository.SaveAsync(user);
            return user;
        }

        public async Task<Customer> GetUserByIdAsync(int userId)
        {
            var user = await _customerRepository.FindByUserIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            return user;
        }

        public async Task<Customer> UpdateUserAsync(int userId, Customer customer)
        {
             var existing = await _customerRepository.FindByUserIdAsync(userId);
             if (existing == null) throw new Exception("User not found");
             
             existing.FullName = customer.FullName;
             existing.Mobile = customer.Mobile;
             existing.BirthDate = customer.BirthDate;
             existing.Interests = customer.Interests;

             if (customer.AddressId != null) existing.AddressId = customer.AddressId;
             
             await _customerRepository.SaveAsync(existing);
             return existing;
        }
    }
}
