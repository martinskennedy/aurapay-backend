using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.DTOs
{
    public record UserDto(Guid Id, string FullName, string Email, string Document);
    public record CreateUserRequestDto(string FullName, string Email, string Document, string Password);
}
