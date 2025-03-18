using Xunit;
using Domain.DTOs;

namespace UnitTests.DTOs
{
    public class EditProfileDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var dto = new EditProfileDto();

            // Assert
            Assert.NotNull(dto);
            Assert.Null(dto.Name);
            Assert.Null(dto.Email);
            Assert.Null(dto.CurrentPassword);
            Assert.Null(dto.NewPassword);
        }

        [Fact]
        public void Properties_ShouldBeSetAndGetCorrectly()
        {
            // Arrange
            var dto = new EditProfileDto();
            var testName = "John Doe";
            var testEmail = "john.doe@example.com";
            var testCurrentPassword = "oldPassword123";
            var testNewPassword = "newPassword456";

            // Act
            dto.Name = testName;
            dto.Email = testEmail;
            dto.CurrentPassword = testCurrentPassword;
            dto.NewPassword = testNewPassword;

            // Assert
            Assert.Equal(testName, dto.Name);
            Assert.Equal(testEmail, dto.Email);
            Assert.Equal(testCurrentPassword, dto.CurrentPassword);
            Assert.Equal(testNewPassword, dto.NewPassword);
        }
    }
}
