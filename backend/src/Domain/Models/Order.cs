namespace Domain.Models
{
    public class Order : IEntity
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryZipCode { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Enviado;
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pendente,
        Enviado,
        Entregue,
        Cancelado
    }
}
