using System;
using Domain.Services;

namespace Domain.Models
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public required string UserPassword { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedLoginAttempts { get; set; }
    }

    public enum UserRole
    {
        ADMINISTRADOR,
        CLIENTE
    }

}
