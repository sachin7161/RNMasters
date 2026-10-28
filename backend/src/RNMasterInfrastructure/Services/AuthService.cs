
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using RNMasterApplication.Authentication.DTOs;
using RNMasterApplication.Authentication.Interfaces;
using RNMasterInfrastructure.Identity;

namespace RNMasterInfrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "User with this email already exists.");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            TenantId = 1
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        const string defaultRole = "User";

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            defaultRole);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                roleResult.Errors.Select(e => e.Description));

            await _userManager.DeleteAsync(user);

            throw new InvalidOperationException(
                $"Failed to assign default role: {errors}");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Token = null,
            RefreshToken = null,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            TenantId = user.TenantId,
            Roles = roles
        };
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(
            request.Email);

        if (user == null ||
            !await _userManager.CheckPasswordAsync(
                user,
                request.Password))
        {
            throw new InvalidOperationException(
                "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = await _jwtTokenService.GenerateTokenAsync(
            user.Id,
            user.Email!,
            user.FullName,
            roles);

        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                updateResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to update refresh token: {errors}");
        }

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            TenantId = user.TenantId,
            Roles = roles
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(
                u => u.RefreshToken == request.RefreshToken);

        if (user == null)
        {
            throw new InvalidOperationException(
                "Invalid refresh token.");
        }

        if (user.RefreshTokenExpiryTime == null ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Refresh token has expired.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken =
            await _jwtTokenService.GenerateTokenAsync(
                user.Id,
                user.Email!,
                user.FullName,
                roles);

        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                updateResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to rotate refresh token: {errors}");
        }

        return new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            TenantId = user.TenantId,
            Roles = roles
        };
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(
                u => u.RefreshToken == request.RefreshToken);

        if (user == null)
        {
            throw new InvalidOperationException(
                "Invalid refresh token.");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                updateResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Failed to logout user: {errors}");
        }
    }
}
