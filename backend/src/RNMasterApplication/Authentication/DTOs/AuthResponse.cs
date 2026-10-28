namespace RNMasterApplication.Authentication.DTOs;

public class AuthResponse
{
    public string? Token { get; set; }

    public string? RefreshToken { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int TenantId { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}