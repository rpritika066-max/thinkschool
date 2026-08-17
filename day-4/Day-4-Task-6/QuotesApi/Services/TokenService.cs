using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Options;

namespace QuotesApi.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly QuoteDbContext _dbContext;
    private readonly IClock _clock;

    public TokenService(
        IOptions<JwtOptions> jwtOptions,
        QuoteDbContext dbContext,
        IClock clock)
    {
        _jwtOptions = jwtOptions.Value;
        _dbContext = dbContext;
        _clock = clock;
    }

    public string GenerateJwt(string username, out string jwtId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);

        jwtId = Guid.NewGuid().ToString();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim("scope", "quotes.write")
            }),

            NotBefore = _clock.UtcNow.UtcDateTime,

            Expires = _clock.UtcNow.UtcDateTime
                .Add(_jwtOptions.AccessTokenLifetime),

            Issuer = _jwtOptions.Issuer,

            Audience = _jwtOptions.Audience,

            SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(string username, string jwtId)
    {
        return new RefreshToken
        {
            JwtId = jwtId,
            UserId = username,
            CreationDate = _clock.UtcNow.UtcDateTime,
            ExpiryDate = _clock.UtcNow.UtcDateTime.AddMonths(1),
            Token = Guid.NewGuid().ToString("N")
        };
    }

    public async Task<bool> RevokeAllUserTokensAsync(string userId)
    {
        var allTokens = await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId)
            .ToListAsync();

        foreach (var t in allTokens)
        {
            t.Invalidated = true;
        }

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<RefreshToken?> ValidateAndProcessRefreshTokenAsync(
        string refreshTokenString)
    {
        var storedRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshTokenString);

        if (storedRefreshToken == null)
            return null;

        if (storedRefreshToken.Used || storedRefreshToken.Invalidated)
        {
            await RevokeAllUserTokensAsync(storedRefreshToken.UserId);
            return null;
        }

        if (storedRefreshToken.ExpiryDate < _clock.UtcNow.UtcDateTime)
            return null;

        storedRefreshToken.Used = true;

        await _dbContext.SaveChangesAsync();

        return storedRefreshToken;
    }
}