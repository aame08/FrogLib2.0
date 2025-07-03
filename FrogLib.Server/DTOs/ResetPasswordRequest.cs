namespace FrogLib.Server.DTOs
{
    public class ResetPasswordRequest
    {
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string UserEmail { get; set; }
    }
}
