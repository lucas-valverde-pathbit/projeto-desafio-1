using Xunit;
using Domain.DTOs;

namespace UnitTests.DTOs
{
    public class SignupRequestTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var signupRequest = new SignupRequest();

            // Assert
            Assert.NotNull(signupRequest);
            Assert.Null(signupRequest.SignupName);
            Assert.Null(signupRequest.SignupEmail);
            Assert.Null(signupRequest.SignupPassword);
            Assert.Null(signupRequest.SignupRole);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var signupRequest = new SignupRequest
            {
                SignupName = "John Doe",
                SignupEmail = "john.doe@example.com",
                SignupPassword = "password123",
                SignupRole = "Admin"
            };

            // Act & Assert
            Assert.Equal("John Doe", signupRequest.SignupName);
            Assert.Equal("john.doe@example.com", signupRequest.SignupEmail);
            Assert.Equal("password123", signupRequest.SignupPassword);
            Assert.Equal("Admin", signupRequest.SignupRole);
        }

        [Fact]
        public void ShouldAllowNullForOptionalProperties()
        {
            // Arrange
            var signupRequest = new SignupRequest
            {
                SignupName = null,
                SignupEmail = null,
                SignupPassword = null,
                SignupRole = null
            };

            // Act & Assert
            Assert.Null(signupRequest.SignupName);
            Assert.Null(signupRequest.SignupEmail);
            Assert.Null(signupRequest.SignupPassword);
            Assert.Null(signupRequest.SignupRole);
        }
    }
}
