namespace Mashkoor.Modules.Users.Commands.Responses;

public sealed record JwtResponse(
    string Name,
    string Username,
    string Token,
    string RefreshToken,
    DateTime TokenExpiry,
    string[] Roles);
