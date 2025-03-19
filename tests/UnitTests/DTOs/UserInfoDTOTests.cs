using Xunit;
using Domain.DTOs; 

namespace UnitTests.DTOs
{
    public class UserInfoDTOTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var userInfo = new UserInfoDTO();

            // Assert
            Assert.NotNull(userInfo);
            Assert.Null(userInfo.NameId);
            Assert.Null(userInfo.Email);
            Assert.Null(userInfo.UniqueName);
            Assert.Null(userInfo.Role);
            Assert.Null(userInfo.CustomerId);
            Assert.Equal(0L, userInfo.Nbf);
            Assert.Equal(0L, userInfo.Exp);
            Assert.Equal(0L, userInfo.Iat);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var userInfo = new UserInfoDTO
            {
                NameId = "John Doe",
                Email = "john.doe@example.com",
                UniqueName = "johndoe123",
                Role = "Admin",
                CustomerId = "cust123",
                Nbf = 1609459200L, // Exemplo de valor para Nbf
                Exp = 1612137600L, // Exemplo de valor para Exp
                Iat = 1609459200L  // Exemplo de valor para Iat
            };

            // Act & Assert
            Assert.Equal("John Doe", userInfo.NameId);
            Assert.Equal("john.doe@example.com", userInfo.Email);
            Assert.Equal("johndoe123", userInfo.UniqueName);
            Assert.Equal("Admin", userInfo.Role);
            Assert.Equal("cust123", userInfo.CustomerId);
            Assert.Equal(1609459200L, userInfo.Nbf);
            Assert.Equal(1612137600L, userInfo.Exp);
            Assert.Equal(1609459200L, userInfo.Iat);
        }

        [Fact]
        public void ShouldAllowNullForOptionalProperties()
        {
            // Arrange
            var userInfo = new UserInfoDTO
            {
                NameId = null,
                Email = null,
                UniqueName = null,
                Role = null,
                CustomerId = null
            };

            // Act & Assert
            Assert.Null(userInfo.NameId);
            Assert.Null(userInfo.Email);
            Assert.Null(userInfo.UniqueName);
            Assert.Null(userInfo.Role);
            Assert.Null(userInfo.CustomerId);
        }

        [Fact]
        public void ShouldAllowLongValuesForTimestampProperties()
        {
            // Arrange
            var userInfo = new UserInfoDTO
            {
                Nbf = 1609459200L,  // Exemplo de valor de timestamp
                Exp = 1612137600L,  // Exemplo de valor de timestamp
                Iat = 1609459200L   // Exemplo de valor de timestamp
            };

            // Act & Assert
            Assert.Equal(1609459200L, userInfo.Nbf);
            Assert.Equal(1612137600L, userInfo.Exp);
            Assert.Equal(1609459200L, userInfo.Iat);
        }
    }
}
