using System;

namespace Domain.DTOs
{
    public class OrderUpdateDTO
    {
        public string? DeliveryAddress { get; set; }
        public string? Status { get; set; }
    }
}
