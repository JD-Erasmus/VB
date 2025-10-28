using System.ComponentModel.DataAnnotations;

namespace VB.Models
{
    public class Vault
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        [Required]
        [StringLength(100)]
        public string WebsiteName { get; set; }

        
    }
}
