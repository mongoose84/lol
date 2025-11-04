using System.Text.Json.Serialization;

namespace RiotProxy.Application.DTOs
{
    // DTOs for request binding
    public record CreateUserRequest(
        [property: JsonPropertyName("accounts")] List<AccountDto> Accounts
    );

    public record AccountDto(
        [property: JsonPropertyName("gameName")] string GameName,
        [property: JsonPropertyName("tagLine")] string TagLine
    );
}