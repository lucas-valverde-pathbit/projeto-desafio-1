namespace Domain.Models
{
    public enum UserRole
    {
        CLIENTE,
        ADMINISTRADOR
    }

    public class User : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public UserRole Role { get; set; } = UserRole.CLIENTE;
        public ICollection<Order>? Orders { get; set; }
    }
}
