using Domain.Models;
using System;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid userId);
        Task<bool> UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
    }
}
