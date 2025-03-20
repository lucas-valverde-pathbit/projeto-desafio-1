using Moq;
using Xunit;
using Domain.Services;

namespace UnitTests.Services
{
    public class PasswordHasherTests
    {
        private readonly Mock<IPasswordHasher> _passwordHasherMock;

        public PasswordHasherTests()
        {
            _passwordHasherMock = new Mock<IPasswordHasher>();
        }

        [Fact]
        public void VerificaSeRetornaComoVerdadeiroQuandoHashCorreto()
        {
            // Arrange
            var password = "correctpassword";
            var hashedPassword = "hashed_password";
            _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(password, hashedPassword)).Returns(true);

            // Act
            var result = _passwordHasherMock.Object.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerificaSeRetornaFalsoQuandoHashIncorreto()
        {
            // Arrange
            var password = "wrongpassword";
            var hashedPassword = "hashed_password";
            _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(password, hashedPassword)).Returns(false);

            // Act
            var result = _passwordHasherMock.Object.VerifyPassword(password, hashedPassword);

            // Assert
            Assert.False(result);
        }
    }
}
