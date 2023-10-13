namespace backend.Authorization;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    
    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var userId = token == null ? null : jwtUtils.ValidateJwtToken(token);
        if (userId != null)
        {
            context.Items["UserId"] = userId.Value; // ключ успешно проверен, добавляем UserId в context
        }

        await _next(context);
    }
    
}