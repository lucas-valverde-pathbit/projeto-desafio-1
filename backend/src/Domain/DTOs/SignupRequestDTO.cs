using System;

namespace Domain.DTOs
{
    public class SignupRequestDTO
    {
        public string SignupName { get; set; }
        public string SignupEmail { get; set; }
        public string SignupPassword { get; set; }
        public string SignupRole { get; set; }
    }
}
