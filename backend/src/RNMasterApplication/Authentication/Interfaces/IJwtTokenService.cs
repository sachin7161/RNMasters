namespace RNMasterApplication.Authentication.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(
        string userId,
        string email,
        string fullName,
        IList<string> roles);
}