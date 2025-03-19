using Xunit;
using Domain.DTOs; 

namespace UnitTests.DTOs
{
    public class UpdateOrderStatusDTOTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperty()
        {
            // Arrange & Act
            var updateOrderStatusDto = new UpdateOrderStatusDto();

            // Assert
            Assert.NotNull(updateOrderStatusDto);
            Assert.Equal(0, updateOrderStatusDto.Status);  // Valor padrão de int é 0
        }

        [Fact]
        public void Status_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var updateOrderStatusDto = new UpdateOrderStatusDto
            {
                Status = 1  // Definindo o status para 1
            };

            // Act & Assert
            Assert.Equal(1, updateOrderStatusDto.Status);  // Verificando se o valor atribuído é o mesmo
        }

        [Fact]
        public void Status_ShouldAllowDifferentValues()
        {
            // Arrange
            var updateOrderStatusDto = new UpdateOrderStatusDto();

            // Act & Assert
            updateOrderStatusDto.Status = 2;
            Assert.Equal(2, updateOrderStatusDto.Status);  // Verificando se o valor atribuído é 2

            updateOrderStatusDto.Status = 3;
            Assert.Equal(3, updateOrderStatusDto.Status);  // Verificando se o valor foi alterado para 3
        }
    }
}
