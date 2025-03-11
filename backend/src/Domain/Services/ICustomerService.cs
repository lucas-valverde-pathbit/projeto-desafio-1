using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Services
{
    public interface ICustomerService : IBaseService<Customer>
    {
        Task<Customer?> GetByEmail(string email);
        Task<Customer?> GetByUserId(Guid userId);
    }
}
