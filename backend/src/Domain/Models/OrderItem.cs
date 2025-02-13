using System;

namespace Domain.Models
{
    public class OrderItem : IEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public int OrderItemQuantity { get; set; }
        public decimal OrderItemPrice { get; set; }
    }
}
