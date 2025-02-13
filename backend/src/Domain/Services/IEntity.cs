using System;

namespace Domain.Models
{
    public interface IEntity
    {
        Guid Id { get; set; }  // A propriedade Id será obrigatória em todas as entidades
    }
}
