namespace FrogLib.Server.DTOs
{
    public class UserDTO
    {
        public string? NameUser { get; set; }
        public string? LoginUser { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
        public IFormFile? ProfileImageUrl { get; set; }
        public string? DeleteImage { get; set; }
    }
}
