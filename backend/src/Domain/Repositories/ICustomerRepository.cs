using Domain.Models;
using System;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> GetByUserIdAsync(Guid userId);
        Task<bool> UpdateAsync(Customer customer);
        Task<IEnumerable<Customer>> GetAllAsync();
    }
}
