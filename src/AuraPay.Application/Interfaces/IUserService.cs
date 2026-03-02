using AuraPay.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraPay.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterUserAsync(CreateUserRequestDto request);
        Task<UserDto?> GetByExternalIdAsync(Guid externalId);
    }
}
