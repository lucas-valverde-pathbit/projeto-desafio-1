namespace Domain.Models
{
    public class Order : IEntity
    {
        public Guid Id { get; set; }  
        public DateTime OrderDate { get; set; }

        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public User? User { get; set; }  // Propriedade de navegação para User

        public string? DeliveryAddress { get; set; }  // Endereço de entrega

        // Alterando a propriedade Status para usar o enum OrderStatus
        public OrderStatus Status { get; set; } = OrderStatus.Pendente;  // Status com valor padrão "Pendente"

        public ICollection<OrderItem>? OrderItems { get; set; }  // Relacionamento com OrderItems
    }

    // Enum que define os possíveis status de um pedido
    public enum OrderStatus
    {
        Pendente,
        Enviado,
        Entregue,
        Cancelado
    }
}

