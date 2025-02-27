using System;

namespace Domain.DTOs
{
public class OrderUpdateDTO
{
    public string? DeliveryAddress { get; set; }
    public string? Status { get; set; }
    public int AddressId { get; set; }
    public string? ZipCode { get; set; }
    public string? ZipCodeFormatted { get; set; }
    public string? AddressType { get; set; }
    public string? AddressName { get; set; }

    }
}
