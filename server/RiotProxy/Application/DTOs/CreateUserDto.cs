using System.Text.Json.Serialization;

namespace RiotProxy.Application.DTOs
{
    // DTOs for request binding
     public record AccountDto(string GameName, string TagLine);
    public record CreateUserRequest(string Password, List<AccountDto> Accounts);
}