using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Domain.Services;
using Domain.DTOs;

namespace Domain.Services
{
    public interface IUserService : IBaseService<User>
    {
        Task<User> Authenticate(string username, string password);
        Task<User?> GetByEmail(string email);
        Task Create(User user);
        Task UpdateUserAsync(Guid userId, string userName, string userEmail, string userPassword, UserRole role, string? customerName, string? customerEmail);
        Task<bool> EditUserProfileAsync(Guid userId, EditProfileDto editProfileDto);
    }


}
