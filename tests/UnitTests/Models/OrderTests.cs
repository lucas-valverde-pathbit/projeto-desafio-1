using System;
using Domain.Models;
using Xunit;

namespace UnitTests.Models
{
    public class OrderTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEmptyOrder()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            Assert.NotNull(order);
            Assert.Equal(Guid.Empty, order.Id); // Verifica se o ID está vazio
            Assert.Equal(DateTime.MinValue, order.OrderDate); // Verifica se a data é mínima (não atribuída)
            Assert.Equal(Guid.Empty, order.CustomerId); // Verifica se o CustomerId está vazio
            Assert.Null(order.Customer); // Verifica se Customer é nulo
            Assert.Null(order.DeliveryAddress); // Verifica se o DeliveryAddress é nulo
            Assert.Null(order.DeliveryZipCode); // Verifica se o DeliveryZipCode é nulo
            Assert.Equal(OrderStatus.Pendente, order.Status); // Verifica se o status padrão é "Pendente"
            Assert.Equal(0, order.TotalAmount); // Verifica se o TotalAmount é 0
            Assert.NotNull(order.OrderItems); // Verifica se a lista de itens do pedido não é nula
            Assert.Empty(order.OrderItems); // Verifica se a lista de itens do pedido está vazia
        }

        [Fact]
        public void OrderStatus_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var newStatus = OrderStatus.Enviado;

            // Act
            order.Status = newStatus;

            // Assert
            Assert.Equal(newStatus, order.Status);
        }

        [Fact]
        public void TotalAmount_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var totalAmount = 100.50m;

            // Act
            order.TotalAmount = totalAmount;

            // Assert
            Assert.Equal(totalAmount, order.TotalAmount);
        }

        [Fact]
        public void DeliveryAddress_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var deliveryAddress = "123 Main St, Some City";

            // Act
            order.DeliveryAddress = deliveryAddress;

            // Assert
            Assert.Equal(deliveryAddress, order.DeliveryAddress);
        }

        [Fact]
        public void DeliveryZipCode_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var zipCode = "12345-678";

            // Act
            order.DeliveryZipCode = zipCode;

            // Assert
            Assert.Equal(zipCode, order.DeliveryZipCode);
        }

        [Fact]
        public void CustomerId_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var customerId = Guid.NewGuid();

            // Act
            order.CustomerId = customerId;

            // Assert
            Assert.Equal(customerId, order.CustomerId);
        }

        [Fact]
        public void Customer_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var customer = new Customer { Id = Guid.NewGuid(), CustomerName = "John Doe" };

            // Act
            order.Customer = customer;

            // Assert
            Assert.Equal(customer, order.Customer);
        }

        [Fact]
        public void OrderItems_ShouldBeSetAndGet()
        {
            // Arrange
            var order = new Order();
            var orderItem = new OrderItem { Id = Guid.NewGuid(), ProductName = "Product 1", Quantity = 2, Price = 50.00m };

            // Act
            order.OrderItems.Add(orderItem);

            // Assert
            Assert.Contains(orderItem, order.OrderItems);
            Assert.Single(order.OrderItems);
        }

        [Fact]
        public void ShouldHaveDefaultStatus_WhenNewOrderIsCreated()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            Assert.Equal(OrderStatus.Pendente, order.Status);
        }
    }
}
