using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        public int? MuseumId { get; set; }
    }
}
