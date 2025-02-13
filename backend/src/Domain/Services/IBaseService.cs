using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Services
{
    public interface IBaseService<T> where T : class, IEntity  // Garante que T implementa IEntity
    {
        Task<T> Add(T entity);  // Adiciona uma nova entidade
        Task<T?> GetById(Guid id);  // Obtém uma entidade pelo ID
        Task<IEnumerable<T>> GetAll();  // Obtém todas as entidades
        Task<T?> Update(Guid id, T entity);  // Atualiza uma entidade existente
        Task<bool> Delete(Guid id);  // Deleta uma entidade pelo ID
    }
}
