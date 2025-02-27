namespace Domain.Models
{
    public class Order : IEntity
    {
        public Guid Id { get; set; }  
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public User? User { get; set; } 
        public string? DeliveryAddress { get; set; }  
        public OrderStatus Status { get; set; } = OrderStatus.Pendente;  

        public ICollection<OrderItem>? OrderItems { get; set; }  
    }

    public enum OrderStatus
    {
        Pendente,
        Enviado,
        Entregue,
        Cancelado
    }
}

