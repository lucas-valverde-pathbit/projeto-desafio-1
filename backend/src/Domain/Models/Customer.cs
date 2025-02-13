using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Customer : IEntity
    {
        public Guid Id { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public ICollection<Order>? Orders { get; set; }  // Coleção de Orders
    }
}
