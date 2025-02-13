using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IRepository<T> where T : class
    {
        Task<T> Add(T entity);
        Task<T?> GetById(Guid id);
        Task<IEnumerable<T>> GetAll();
        Task<T?> Update(Guid id, T entity);
        Task<bool> Delete(Guid id);
    }
}
