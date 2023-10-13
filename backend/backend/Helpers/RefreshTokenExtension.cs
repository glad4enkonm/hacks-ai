using database.Models;

namespace backend.Helpers;

public static class RefreshTokenExtension
{
    public static bool IsExpired(this RefreshToken token)
    {
        return DateTime.UtcNow >=  token.Expires;
    }
    
    public static bool IsRevoked(this RefreshToken token)
    {
        return token.Revoked.HasValue;
    }
    
    public static bool IsActive(this RefreshToken token)
    {
        return  !token.IsRevoked() && !token.IsExpired();
    }
}