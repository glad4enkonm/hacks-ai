namespace backend.Helpers;

public class AppSettings
{
    // Время действия ключей доступа в минутах
    public int AccessTokenDurationInMin { get; set; }
    
    // Время действия ключей обновления в днях
    public int RefreshTokenDurationInDays { get; set; }
    
    // Время храниения старых refresh ключей для отслеживания подозрительных обновлений
    public int RefreshTokenTtl { get; set; }
}