namespace Domain.Models
{
    public class User : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();  // Identificador único do usuário
        public string? UserEmail { get; set; }  // Email do usuário
        public string? UserPassword { get; set; }  // Senha do usuário
        public string Role { get; set; } = "CLIENTE";  // Definindo o papel padrão do usuário como "CLIENTE"

        // Relacionamento com Order: um User pode ter vários Orders
        public ICollection<Order>? Orders { get; set; }  // Propriedade de navegação para Orders
    }
}
