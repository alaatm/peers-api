namespace Peers.Core.Nafath.Models;

public sealed record NafathIdentity(
    string NationalId,
    string? FirstNameAr,
    string? LastNameAr,
    string? FirstNameEn,
    string? LastNameEn,
    string? Gender);
