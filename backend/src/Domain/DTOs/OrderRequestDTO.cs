namespace Domain.DTOs
{
    public class OrderRequestDTO
    {
        public Guid CustomerId { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryZipCode { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }

    public class OrderItemDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
