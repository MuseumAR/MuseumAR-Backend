using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        public int? MuseumId { get; set; }
        
        public string? Password { get; set; } // Optional: to change password
    }
}
