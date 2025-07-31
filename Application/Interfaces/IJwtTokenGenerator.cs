using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuario usuario);
}