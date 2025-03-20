using System;

namespace Domain.DTOs
{
        public class LoginRequestDTO
        {
            public required string LoginEmail { get; set; }
            public required string LoginPassword { get; set; }
        }
}
