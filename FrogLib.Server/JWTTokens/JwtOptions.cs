namespace FrogLib.Server.JWTTokens
{
    public class JwtOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public int ExpitesHours { get; set; } = 1;
    }
}
